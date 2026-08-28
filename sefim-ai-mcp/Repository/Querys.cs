namespace SefimMcp.Repository;

public static class Querys
{
    #region Repo

    public static string ProductListQuery = 
        @$" SELECT Id, ProductName, ProductGroup, ProductCode, [Order], Price, VatRate, Plu, IsSynced FROM dbo.Product
            WHERE ProductName LIKE @search OR ProductGroup LIKE @search OR 
            ProductCode LIKE @search OR Price LIKE @search OR VatRate LIKE @search 
            ";

    public static string CustomerListQuery =
        $@"
        SELECT Id, CustomerName, FullName, CustomerCode, Adi, Soyadi, Eposta, TaxOffice, TaxNumber, Address1, Address2, Category, PhoneNumber, DiscountRate, Passive, CreditAllowance, CardNo, Aktarildi, IsSynced, IsUpdated FROM dbo.Customer
        WHERE CustomerName LIKE @Search OR FullName LIKE @Search OR CustomerCode LIKE @Search OR Adi LIKE @Search OR Soyadi LIKE @Search OR Eposta LIKE @Search OR TaxOffice LIKE @Search OR TaxNumber LIKE @Search OR
        Address1 LIKE @Search OR Address2 LIKE @Search OR Category LIKE @Search OR PhoneNumber LIKE @Search OR CardNo LIKE @Search OR
          ";
    
    public static string SoftDeleteCustomerQuery =
        $@"
        UPDATE dbo.Customer
        SET Passive = 1
        WHERE Id = @P1
          ";
    
    public static string ListUserQuery =
        $@"
        SELECT Id, UserName, [Role] FROM dbo.[User] WHERE UserName LIKE @Search OR [Role] LIKE @Search
          ";
    
    public static string ListUserProductQuery =
        $@"
        SELECT Id, UserName, ProductId FROM dbo.[UserProducts] WHERE UserName LIKE @Search
          ";
    
    public static string ListPermissionQuery =
        $@"
        SELECT Id, UserName, PermissionName, PermissionValue, Aktarildi, IsSynced, IsUpdated FROM [dbo].[Permission]
        WHERE UserName LIKE @Search OR PermissionName LIKE @Search
          ";
    
    public static string ListMenuQuery =
        $@"
        SELECT Id, MenuName, ChoiceCount, Price, Active, Aktarildi, IsSynced, IsUpdated FROM dbo.Menu
        WHERE MenuName LIKE @Search
          ";
    
    public static string ListMenuProductQuery =
        $@"
        SELECT Id, MenuId, ProductName, Price, Aktarildi, IsSynced, IsUpdated, ProductId FROM dbo.MenuProduct
        WHERE ProductName LIKE @Search
          ";

    #endregion

    #region Report

    private const string EndOfTheDayCommonCte =
        """
        WITH
        IncludedUsers AS (
            SELECT DISTINCT
                UserName AS UNJ
            FROM [User]
            WHERE
                ISNULL([User].Branch, '') IN ('') AND
                ISNULL([User].Department, '') IN ('')
        ),
        PaidBillx AS (SELECT PaidBill.* FROM PaidBill LEFT JOIN IncludedUsers ON PaidBill.UserName = IncludedUsers.UNJ),
        Billx AS (SELECT Bill.* FROM Bill LEFT JOIN IncludedUsers ON Bill.UserName = IncludedUsers.UNJ),
        BillHeaderx AS (SELECT BillHeader.* FROM BillHeader WHERE BillHeader.Id IN (SELECT DISTINCT HeaderId FROM Billx)),
        DeletedBillx AS (SELECT DeletedBill.* FROM DeletedBill LEFT JOIN IncludedUsers ON DeletedBill.UserName = IncludedUsers.UNJ),
        DebitPaymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName = IncludedUsers.UNJ WHERE Payment.TableNo = 'DEBIT' AND Payment.Debit = 0),
        Collectx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName = IncludedUsers.UNJ WHERE ISNULL(FastSale, 0) = 0 AND Payment.TableNo <> 'DEBIT'),
        DirectTransactionx AS (SELECT DirectTransaction.* FROM DirectTransaction LEFT JOIN IncludedUsers ON DirectTransaction.UserName = IncludedUsers.UNJ),
        PhoneOrderHeaderx AS (SELECT PhoneOrderHeader.* FROM PhoneOrderHeader LEFT JOIN IncludedUsers ON PhoneOrderHeader.CreatedByUserName = IncludedUsers.UNJ),
        Paymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName = IncludedUsers.UNJ WHERE ISNULL(FastSale, 0) = 1 AND Payment.TableNo <> 'DEBIT'),
        PrePaymentx AS (SELECT PrePayment.* FROM PrePayment LEFT JOIN IncludedUsers ON PrePayment.UserName = IncludedUsers.UNJ WHERE PrePayment.TableNumber <> 'DEBIT'),
        BillWithHeaderx AS (
            SELECT
                BillWithHeader.*,
                Product.ProductCode,
                Product.StockCode,
                BillWithHeader.Price / (1 + (BillWithHeader.VatRate / 100.0)) AS PriceWithoutVAT,
                BillWithHeader.Price - (BillWithHeader.Price / (1 + (BillWithHeader.VatRate / 100.0))) AS VATAmount
            FROM BillWithHeader
            LEFT JOIN IncludedUsers ON BillWithHeader.UserName = IncludedUsers.UNJ
            LEFT JOIN Product ON BillWithHeader.ProductId = Product.Id
            WHERE BillWithHeader.ProductName NOT LIKE '%\[R]%' ESCAPE '\'
        )
        """;

    public static readonly string EndOfTheDayQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte}
        SELECT
            'Restoran' AS Source,
            TableNo,
            Debit,
            PaymentTime,
            ReceivedByUserName,
            HeaderId AS [Key],
            CustomerName
        FROM Paymentx
        WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2 AND ISNULL(Debit, 0) <> 0

        UNION

        SELECT
            'Paketçi' AS Source,
            'PAKET' AS TableNo,
            (
                SELECT SUM(Price * Quantity)
                FROM Billx
                WHERE Billx.HeaderId = PhoneOrderHeaderx.HeaderId
            ) - PhoneOrderHeaderx.Discount AS Debit,
            CreationTime AS PaymentTime,
            CreatedByUserName AS ReceivedByUserName,
            PhoneOrderHeaderx.HeaderId AS [Key],
            P.CustomerName
        FROM PhoneOrderHeaderx
        LEFT JOIN Payment P ON P.HeaderId = PhoneOrderHeaderx.HeaderId
        WHERE [CreationTime] >= @par1 AND [CreationTime] < @par2 AND Paid = 0
        ORDER BY [Key];

        SELECT *
        FROM DeletedBill
        WHERE [DeletingTime] >= @par1 AND [DeletingTime] <= @par2;

        {EndOfTheDayCommonCte}
        SELECT
            COUNT(*) AS TotalPaymentCount,
            SUM(CashPayment) AS CashPayment,
            SUM(CreditPayment) AS CreditPayment,
            SUM(TicketPayment) AS TicketPayment,
            SUM(OnlinePayment) AS OnlinePayment,
            SUM(Discount) AS Discount,
            SUM(Debit) AS Debit,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                JOIN Product ON Product.Id = PaidBillx.ProductId
                WHERE Product.ProductType = 'OransalKuver'
                  AND [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
            ) AS ServiceTotal,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND ISNULL(Ikram, 0) = 1
            ) AS Free,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND ISNULL(Zayi, 0) = 1
            ) AS Zayi,
            (
                SELECT SUM(OriginalPrice * ABS(Quantity))
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND PaidBillx.FastSale = 1
                  AND ISNULL(Quantity, 0) < 0
            ) AS ReturnSum,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM DeletedBillx
                WHERE [Date] >= @par1 AND [Date] <= @par2
            ) AS Canceled,
            (
                SELECT SUM(Billx.Price * Billx.Quantity)
                FROM Billx
                JOIN BillHeader AS H ON H.Id = Billx.HeaderId
                WHERE H.BillState = 0 AND H.BillType IN (0, 1)
            ) AS OpenOrders,
            (
                SELECT SUM(OriginalPrice * ABS(Quantity))
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND PaidBillx.FastSale = 0
                  AND ISNULL(Quantity, 0) < 0
            ) AS PakReturnsUM,
            (SELECT SUM(CashPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_CashPayment,
            (SELECT SUM(CreditPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_CreditPayment,
            (SELECT SUM(TicketPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_TicketPayment,
            (SELECT SUM(OnlinePayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_OnlinePayment,
            (SELECT SUM(CashPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_CashPayment,
            (SELECT SUM(CreditPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_CreditPayment,
            (SELECT SUM(TicketPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_TicketPayment,
            (SELECT SUM(OnlinePayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_OnlinePayment,
            (SELECT SUM(Discount) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_Discount,
            (SELECT SUM(Total) FROM DirectTransactionx WHERE [Date] >= @par1 AND [Date] < @par2) AS DirectTransaction_CashPayment,
            (
                SELECT SUM(b.Price * b.Quantity)
                FROM PhoneOrderHeaderx
                LEFT JOIN Billx B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
                WHERE Paid = 0 AND CreationTime >= @par1 AND CreationTime < @par2
            )
            -
            (
                SELECT SUM(Discount)
                FROM PhoneOrderHeaderx
                WHERE Paid = 0 AND CreationTime >= @par1 AND CreationTime < @par2
            ) AS PhoneOrderDebit,
            (
                SELECT SUM(Discount)
                FROM PhoneOrderHeaderx
                WHERE CreationTime >= @par1 AND CreationTime < @par2
            ) AS PhoneOrder_Discounts
        FROM Paymentx
        WHERE PaymentTime >= @par1 AND PaymentTime <= @par2;

        {EndOfTheDayCommonCte}
        SELECT
            SUM(Price * Quantity) AS Total,
            UserName
        FROM PaidBillx
        WHERE PaymentTime >= @par1 AND PaymentTime < @par2
        GROUP BY UserName
        ORDER BY SUM(Price * Quantity) DESC;

        {EndOfTheDayCommonCte}
        SELECT TOP 1000
            SUM(Quantity) AS Quantity,
            SUM(Quantity * Price) AS Total,
            ProductName
        FROM PaidBillx
        WHERE PaymentTime >= @par1 AND PaymentTime < @par2
        GROUP BY ProductName
        ORDER BY SUM(Quantity) DESC;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM PaidBillx
        WHERE [PaymentTime] >= @par1
          AND [PaymentTime] <= @par2
          AND ISNULL(Ikram, 0) = 1
        ORDER BY Id;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM DebitPaymentx
        WHERE [PaymentTime] >= @par1 AND [PaymentTime] <= @par2;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM Paymentx
        WHERE [PaymentTime] >= @par1
          AND [PaymentTime] <= @par2
          AND ISNULL(Discount, 0) <> 0;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM DirectTransactionx
        WHERE [Date] >= @par1 AND [Date] <= @par2
        ORDER BY [Id];

        {EndOfTheDayCommonCte}
        SELECT
            Id,
            Date,
            UserName,
            ProductName,
            ProductId,
            Choice1Id,
            Choice2Id,
            Options,
            Price,
            ABS(Quantity) AS Quantity,
            Comment,
            HeaderId,
            OrderId
        FROM PaidBillx
        WHERE [PaymentTime] >= @par1
          AND [PaymentTime] <= @par2
          AND ISNULL(Quantity, 0) < 0
        ORDER BY Id;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM PaidBillx
        WHERE [PaymentTime] >= @par1
          AND [PaymentTime] <= @par2
          AND ISNULL(Zayi, 0) = 1
        ORDER BY Id;

        {EndOfTheDayCommonCte}
        SELECT
            SUM(CashPayment) AS CashPayments,
            SUM(CreditPayment) AS CreditPayments,
            SUM(TicketPayment) AS TicketPayments,
            SUM(OnlinePayment) AS OnlinePayments,
            SUM(Discount) AS Discounts
        FROM PrePaymentx
        WHERE PaymentTime >= @par1 AND PaymentTime <= @par2;
        """;

    public const string DeleteFullyCanceledBillsQuery =
        """
        DELETE FROM [dbo].[Bill]
        WHERE [dbo].[Bill].Id IN (
            SELECT BillId
            FROM (
                SELECT D.BillId, SUM(D.Quantity) AS Quantity
                FROM [dbo].[DeletedBill] D
                GROUP BY D.BillId
            ) T
            INNER JOIN [dbo].[Bill] B ON B.Id = T.BillId
            WHERE T.Quantity = B.Quantity
        );
        """;

    public static readonly string DailyTotalsQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte},
        Days AS (
            SELECT TOP (DATEDIFF(DAY, CONVERT(date, @par1), CONVERT(date, @par2)) + 1)
                ROW_NUMBER() OVER (ORDER BY A.object_id) AS RowNo
            FROM sys.objects AS A
            CROSS JOIN sys.objects AS C
            CROSS JOIN sys.objects AS D
        ),
        Shifts AS (
            SELECT
                DATEADD(DAY, Days.RowNo - 1, CONVERT(datetime, CONVERT(date, @par1))) AS sStart,
                DATEADD(DAY, Days.RowNo, CONVERT(datetime, CONVERT(date, @par1))) AS sEnd
            FROM Days
        )
        SELECT
            Shifts.sStart AS Date,
            p.TotalPaymentCount,
            p.CashPayment,
            p.CreditPayment,
            p.TicketPayment,
            p.OnlinePayment,
            p.Discount,
            p.Debit,
            Free.Free,
            ResReturnItems.ResReturnItems,
            Canceled.Canceled,
            DebitPayments.DebitPayment_CashPayment,
            DebitPayments.DebitPayment_CreditPayment,
            DebitPayments.DebitPayment_Discount,
            DebitPayments.DebitPayment_TicketPayment,
            Collects.Collect_CashPayment,
            Collects.Collect_CreditPayment,
            Collects.Collect_Discount,
            Collects.Collect_TicketPayment,
            Collects.Collect_OnlinePayment,
            PakReturnItems.PakReturnItems,
            DirectTransactions.DirectTransaction_CashPayment
        FROM Shifts
        CROSS APPLY (
            SELECT
                COUNT(*) AS TotalPaymentCount,
                SUM(CashPayment) AS CashPayment,
                SUM(CreditPayment) AS CreditPayment,
                SUM(TicketPayment) AS TicketPayment,
                SUM(OnlinePayment) AS OnlinePayment,
                SUM(Discount) AS Discount,
                SUM(Debit) AS Debit
            FROM Paymentx
            WHERE Paymentx.PaymentTime >= Shifts.sStart
              AND Paymentx.PaymentTime < Shifts.sEnd
        ) AS p
        CROSS APPLY (
            SELECT SUM(OriginalPrice * Quantity) AS Free
            FROM PaidBillx
            WHERE PaidBillx.PaymentTime >= Shifts.sStart
              AND PaidBillx.PaymentTime < Shifts.sEnd
              AND ISNULL(Price, 0) = 0
        ) AS Free
        CROSS APPLY (
            SELECT SUM(OriginalPrice * ABS(Quantity)) AS ResReturnItems
            FROM PaidBillx
            WHERE PaidBillx.PaymentTime >= Shifts.sStart
              AND PaidBillx.PaymentTime < Shifts.sEnd
              AND PaidBillx.FastSale = 1
              AND ISNULL(Quantity, 0) < 0
        ) AS ResReturnItems
        CROSS APPLY (
            SELECT SUM(OriginalPrice * Quantity) AS Canceled
            FROM DeletedBillx
            WHERE DeletedBillx.Date >= Shifts.sStart
              AND DeletedBillx.Date < Shifts.sEnd
        ) AS Canceled
        CROSS APPLY (
            SELECT
                SUM(CashPayment) AS DebitPayment_CashPayment,
                SUM(Discount) AS DebitPayment_Discount,
                SUM(CreditPayment) AS DebitPayment_CreditPayment,
                SUM(TicketPayment) AS DebitPayment_TicketPayment
            FROM DebitPaymentx
            WHERE DebitPaymentx.PaymentTime >= Shifts.sStart
              AND DebitPaymentx.PaymentTime < Shifts.sEnd
        ) AS DebitPayments
        CROSS APPLY (
            SELECT
                SUM(CashPayment) AS Collect_CashPayment,
                SUM(CreditPayment) AS Collect_CreditPayment,
                SUM(TicketPayment) AS Collect_TicketPayment,
                SUM(OnlinePayment) AS Collect_OnlinePayment,
                SUM(Discount) AS Collect_Discount
            FROM Collectx
            WHERE Collectx.PaymentTime >= Shifts.sStart
              AND Collectx.PaymentTime < Shifts.sEnd
        ) AS Collects
        CROSS APPLY (
            SELECT SUM(OriginalPrice * ABS(Quantity)) AS PakReturnItems
            FROM PaidBillx
            WHERE PaidBillx.PaymentTime >= Shifts.sStart
              AND PaidBillx.PaymentTime < Shifts.sEnd
              AND PaidBillx.FastSale = 0
              AND ISNULL(Quantity, 0) < 0
        ) AS PakReturnItems
        CROSS APPLY (
            SELECT SUM(Total) AS DirectTransaction_CashPayment
            FROM DirectTransactionx
            WHERE DirectTransactionx.[Date] >= Shifts.sStart
              AND DirectTransactionx.[Date] < Shifts.sEnd
        ) AS DirectTransactions
        ORDER BY Shifts.sStart;
        """;

    public static readonly string RevenueSummaryQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM Paymentx
        WHERE [PaymentTime] >= @par1 AND [PaymentTime] <= @par2;

        {EndOfTheDayCommonCte}
        SELECT
            COUNT(*) AS TotalPaymentCount,
            SUM(CashPayment) AS CashPayment,
            SUM(CreditPayment) AS CreditPayment,
            SUM(TicketPayment) AS TicketPayment,
            SUM(OnlinePayment) AS OnlinePayment,
            SUM(Discount) AS Discount,
            SUM(Debit) AS Debit,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                JOIN Product ON Product.Id = PaidBillx.ProductId
                WHERE Product.ProductType = 'OransalKuver'
                  AND [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
            ) AS ServiceTotal,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND ISNULL(Ikram, 0) = 1
            ) AS Free,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND ISNULL(Zayi, 0) = 1
            ) AS Zayi,
            (
                SELECT SUM(OriginalPrice * ABS(Quantity))
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND PaidBillx.FastSale = 1
                  AND ISNULL(Quantity, 0) < 0
            ) AS ReturnSum,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM DeletedBillx
                WHERE [Date] >= @par1 AND [Date] <= @par2
            ) AS Canceled,
            (
                SELECT SUM(Billx.Price * Billx.Quantity)
                FROM Billx
                JOIN BillHeader AS H ON H.Id = Billx.HeaderId
                WHERE H.BillState = 0 AND H.BillType IN (0, 1)
            ) AS OpenOrders,
            (
                SELECT SUM(OriginalPrice * ABS(Quantity))
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND PaidBillx.FastSale = 0
                  AND ISNULL(Quantity, 0) < 0
            ) AS PakReturnsUM,
            (SELECT SUM(CashPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_CashPayment,
            (SELECT SUM(CreditPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_CreditPayment,
            (SELECT SUM(TicketPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_TicketPayment,
            (SELECT SUM(OnlinePayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_OnlinePayment,
            (SELECT SUM(CashPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_CashPayment,
            (SELECT SUM(CreditPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_CreditPayment,
            (SELECT SUM(TicketPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_TicketPayment,
            (SELECT SUM(OnlinePayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_OnlinePayment,
            (SELECT SUM(Discount) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_Discount,
            (SELECT SUM(Total) FROM DirectTransactionx WHERE [Date] >= @par1 AND [Date] < @par2) AS DirectTransaction_CashPayment,
            (
                SELECT SUM(b.Price * b.Quantity)
                FROM PhoneOrderHeaderx
                LEFT JOIN Billx B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
                WHERE Paid = 0 AND CreationTime >= @par1 AND CreationTime < @par2
            )
            -
            (
                SELECT SUM(Discount)
                FROM PhoneOrderHeaderx
                WHERE Paid = 0 AND CreationTime >= @par1 AND CreationTime < @par2
            ) AS PhoneOrderDebit,
            (
                SELECT SUM(Discount)
                FROM PhoneOrderHeaderx
                WHERE CreationTime >= @par1 AND CreationTime < @par2
            ) AS PhoneOrder_Discounts
        FROM Paymentx
        WHERE PaymentTime >= @par1 AND PaymentTime <= @par2;
        """;

    public static readonly string TableRevenueQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM Paymentx
        WHERE [PaymentTime] >= @par1 AND [PaymentTime] <= @par2;

        {EndOfTheDayCommonCte}
        SELECT
            COUNT(*) AS TotalPaymentCount,
            SUM(CashPayment) AS CashPayment,
            SUM(CreditPayment) AS CreditPayment,
            SUM(TicketPayment) AS TicketPayment,
            SUM(OnlinePayment) AS OnlinePayment,
            SUM(Discount) AS Discount,
            SUM(Debit) AS Debit,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                JOIN Product ON Product.Id = PaidBillx.ProductId
                WHERE Product.ProductType = 'OransalKuver'
                  AND [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
            ) AS ServiceTotal,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND ISNULL(Ikram, 0) = 1
            ) AS Free,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND ISNULL(Zayi, 0) = 1
            ) AS Zayi,
            (
                SELECT SUM(OriginalPrice * ABS(Quantity))
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND PaidBillx.FastSale = 1
                  AND ISNULL(Quantity, 0) < 0
            ) AS ReturnSum,
            (
                SELECT SUM(OriginalPrice * Quantity)
                FROM DeletedBillx
                WHERE [Date] >= @par1 AND [Date] <= @par2
            ) AS Canceled,
            (
                SELECT SUM(Billx.Price * Billx.Quantity)
                FROM Billx
                JOIN BillHeader AS H ON H.Id = Billx.HeaderId
                WHERE H.BillState = 0 AND H.BillType IN (0, 1)
            ) AS OpenOrders,
            (
                SELECT SUM(OriginalPrice * ABS(Quantity))
                FROM PaidBillx
                WHERE [PaymentTime] >= @par1
                  AND [PaymentTime] <= @par2
                  AND PaidBillx.FastSale = 0
                  AND ISNULL(Quantity, 0) < 0
            ) AS PakReturnsUM,
            (SELECT SUM(CashPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_CashPayment,
            (SELECT SUM(CreditPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_CreditPayment,
            (SELECT SUM(TicketPayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_TicketPayment,
            (SELECT SUM(OnlinePayment) FROM DebitPaymentx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS DebitPayment_OnlinePayment,
            (SELECT SUM(CashPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_CashPayment,
            (SELECT SUM(CreditPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_CreditPayment,
            (SELECT SUM(TicketPayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_TicketPayment,
            (SELECT SUM(OnlinePayment) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_OnlinePayment,
            (SELECT SUM(Discount) FROM Collectx WHERE [PaymentTime] >= @par1 AND [PaymentTime] < @par2) AS Collect_Discount,
            (SELECT SUM(Total) FROM DirectTransactionx WHERE [Date] >= @par1 AND [Date] < @par2) AS DirectTransaction_CashPayment,
            (
                SELECT SUM(b.Price * b.Quantity)
                FROM PhoneOrderHeaderx
                LEFT JOIN Billx B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
                WHERE Paid = 0 AND CreationTime >= @par1 AND CreationTime < @par2
            )
            -
            (
                SELECT SUM(Discount)
                FROM PhoneOrderHeaderx
                WHERE Paid = 0 AND CreationTime >= @par1 AND CreationTime < @par2
            ) AS PhoneOrderDebit,
            (
                SELECT SUM(Discount)
                FROM PhoneOrderHeaderx
                WHERE CreationTime >= @par1 AND CreationTime < @par2
            ) AS PhoneOrder_Discounts
        FROM Paymentx
        WHERE PaymentTime >= @par1 AND PaymentTime <= @par2;
        """;

    public static readonly string PersonnelCollectionQuery = RevenueSummaryQuery;

    public static readonly string ActivityReportQuery = RevenueSummaryQuery;

    public static readonly string IncomeExpenseRecordsQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte}
        SELECT *
        FROM DirectTransactionx
        WHERE [Date] >= @par1 AND [Date] <= @par2;
        """;

    public const string DeletedOrdersQuery =
        """
        SET NOCOUNT ON;

        SELECT *
        FROM DeletedBill
        WHERE [DeletingTime] >= @par1 AND [DeletingTime] <= @par2;
        """;

    public static readonly string SalesReportQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte}
        SELECT
            *,
            ((Price * Quantity) - [dbo].[getDiscounRate]([HeaderId]) * (Price * Quantity) / 100) AS NetTutar
        FROM BillWithHeaderx
        WHERE [Date] >= @par1
          AND [Date] < @par2
          AND (BillType = @par3 OR BillType = @par4 OR BillType = @par5);
        """;

    public static readonly string SalesTotalsQuery =
        $"""
        SET NOCOUNT ON;

        {EndOfTheDayCommonCte}
        SELECT
            ProductName,
            SUM(Price * Quantity) AS Total,
            SUM(Quantity) AS Quantity,
            ProductCode,
            StockCode,
            SUM(PriceWithoutVAT * Quantity) AS PriceWithoutVAT,
            SUM(VATAmount * Quantity) AS VATAmount
        FROM BillWithHeaderx
        WHERE [Date] >= @par1
          AND [Date] < @par2
          AND (BillType = @par3 OR BillType = @par4 OR BillType = @par5)
        GROUP BY ProductName, ProductCode, StockCode;
        """;

    #endregion
}
