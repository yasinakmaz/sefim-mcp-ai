using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SefimMcp.Models;

public partial class SefimmContext : DbContext
{
    public SefimmContext(DbContextOptions<SefimmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AddedCoverredWithProduct> AddedCoverredWithProducts { get; set; }

    public virtual DbSet<AppVer> AppVers { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillHeader> BillHeaders { get; set; }

    public virtual DbSet<BillHeaderLog> BillHeaderLogs { get; set; }

    public virtual DbSet<BillLog> BillLogs { get; set; }

    public virtual DbSet<BillPrinter> BillPrinters { get; set; }

    public virtual DbSet<BillWithHeader> BillWithHeaders { get; set; }

    public virtual DbSet<BillWithHeaderOkc> BillWithHeaderOkcs { get; set; }

    public virtual DbSet<Bom> Boms { get; set; }

    public virtual DbSet<BomOption> BomOptions { get; set; }

    public virtual DbSet<CallLog> CallLogs { get; set; }

    public virtual DbSet<CallLog2> CallLog2s { get; set; }

    public virtual DbSet<CampaignDetail> CampaignDetails { get; set; }

    public virtual DbSet<CampaignHeader> CampaignHeaders { get; set; }

    public virtual DbSet<CashierState> CashierStates { get; set; }

    public virtual DbSet<Choice1> Choice1s { get; set; }

    public virtual DbSet<Choice2> Choice2s { get; set; }

    public virtual DbSet<Collect> Collects { get; set; }

    public virtual DbSet<CollectPaid> CollectPaids { get; set; }

    public virtual DbSet<Cost> Costs { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DebitPayment> DebitPayments { get; set; }

    public virtual DbSet<DefaultOption> DefaultOptions { get; set; }

    public virtual DbSet<DeletedBill> DeletedBills { get; set; }

    public virtual DbSet<Deliverer> Deliverers { get; set; }

    public virtual DbSet<DigiPan> DigiPans { get; set; }

    public virtual DbSet<DirectTransaction> DirectTransactions { get; set; }

    public virtual DbSet<DiscountDetail> DiscountDetails { get; set; }

    public virtual DbSet<EfaturaDurumKodlari> EfaturaDurumKodlaris { get; set; }

    public virtual DbSet<FormalBillPrinter> FormalBillPrinters { get; set; }

    public virtual DbSet<MarchingProduct> MarchingProducts { get; set; }

    public virtual DbSet<MaterialTransaction> MaterialTransactions { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuProduct> MenuProducts { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<OptionCat> OptionCats { get; set; }

    public virtual DbSet<OptionsEqu> OptionsEqus { get; set; }

    public virtual DbSet<OrderHeader> OrderHeaders { get; set; }

    public virtual DbSet<OrderPrinter> OrderPrinters { get; set; }

    public virtual DbSet<PaidBill> PaidBills { get; set; }

    public virtual DbSet<PaketEntegrasyon> PaketEntegrasyons { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentDetail> PaymentDetails { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentPaket> PaymentPakets { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PhoneDictionary> PhoneDictionaries { get; set; }

    public virtual DbSet<PhoneOrderHeader> PhoneOrderHeaders { get; set; }

    public virtual DbSet<PhoneOrderJson> PhoneOrderJsons { get; set; }

    public virtual DbSet<PrePayment> PrePayments { get; set; }

    public virtual DbSet<PrintJob> PrintJobs { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductEqu> ProductEqus { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductLongName> ProductLongNames { get; set; }

    public virtual DbSet<ProductTemplate> ProductTemplates { get; set; }

    public virtual DbSet<ProductTemplateAutomation> ProductTemplateAutomations { get; set; }

    public virtual DbSet<ProductTemplatePrice> ProductTemplatePrices { get; set; }

    public virtual DbSet<Production> Productions { get; set; }

    public virtual DbSet<ReasonList> ReasonLists { get; set; }

    public virtual DbSet<ReservationList> ReservationLists { get; set; }

    public virtual DbSet<SayimBaslik> SayimBasliks { get; set; }

    public virtual DbSet<SayimDetay> SayimDetays { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<StockLevel> StockLevels { get; set; }

    public virtual DbSet<SubeLink> SubeLinks { get; set; }

    public virtual DbSet<SyncTable> SyncTables { get; set; }

    public virtual DbSet<TableGroup> TableGroups { get; set; }

    public virtual DbSet<TableLock> TableLocks { get; set; }

    public virtual DbSet<Tblpbipuanyetki> Tblpbipuanyetkis { get; set; }

    public virtual DbSet<Tblpospuan> Tblpospuans { get; set; }

    public virtual DbSet<Tblpospuanaciklama> Tblpospuanaciklamas { get; set; }

    public virtual DbSet<TemplateOverride> TemplateOverrides { get; set; }

    public virtual DbSet<TeraPosDef> TeraPosDefs { get; set; }

    public virtual DbSet<TriggerTable> TriggerTables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<UserProduct> UserProducts { get; set; }

    public virtual DbSet<Value> Values { get; set; }

    public virtual DbSet<VerInfo> VerInfos { get; set; }

    public virtual DbSet<Waste> Wastes { get; set; }

    public virtual DbSet<WeighingProduct> WeighingProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AddedCoverredWithProduct>(entity =>
        {
            entity.Property(e => e.ProductName).HasMaxLength(250);
        });

        modelBuilder.Entity<AppVer>(entity =>
        {
            entity.HasKey(e => e.Name);

            entity.ToTable("AppVer");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Value).IsUnicode(false);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Bills");

            entity.ToTable("Bill", tb => tb.HasTrigger("tg_DelectedBill"));

            entity.HasIndex(e => e.Date, "IX_Bill_DATE");

            entity.HasIndex(e => e.HeaderId, "IX_Bill_HeaderId");

            entity.HasIndex(e => e.OrderId, "IX_Bill_OrderId");

            entity.HasIndex(e => e.ProductName, "IX_Bill_PRODUCTNAME");

            entity.Property(e => e.CampaingGuid).HasMaxLength(50);
            entity.Property(e => e.Comment).HasMaxLength(250);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ExtOrderNo).HasMaxLength(255);
            entity.Property(e => e.IsReady).HasColumnName("isReady");
            entity.Property(e => e.IsSee).HasColumnName("isSee");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ReadyDate).HasColumnType("datetime");
            entity.Property(e => e.UserK).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<BillHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OrderHeader");

            entity.ToTable("BillHeader", tb => tb.HasTrigger("tg_DelectedBillHeader"));

            entity.Property(e => e.BeginTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CloseTime).HasColumnType("datetime");
            entity.Property(e => e.DailyBillNumber).HasMaxLength(255);
            entity.Property(e => e.Fisno)
                .HasMaxLength(100)
                .HasDefaultValue("1");
            entity.Property(e => e.FormalBillId).HasMaxLength(255);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PavoOrderNo).HasMaxLength(50);
            entity.Property(e => e.TableGroupId).HasMaxLength(255);
            entity.Property(e => e.TableNumber).HasMaxLength(250);
        });

        modelBuilder.Entity<BillHeaderLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("BillHeader_LOG");

            entity.Property(e => e.DailyBillNumber).HasMaxLength(255);
            entity.Property(e => e.DeleteDate).HasColumnType("datetime");
            entity.Property(e => e.Fisno).HasMaxLength(100);
            entity.Property(e => e.FormalBillId).HasMaxLength(255);
            entity.Property(e => e.TableGroupId).HasMaxLength(255);
            entity.Property(e => e.TableNumber).HasMaxLength(250);
        });

        modelBuilder.Entity<BillLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Bills_log");

            entity.ToTable("Bill_LOG");

            entity.Property(e => e.Comment).HasMaxLength(250);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DeleteDate).HasColumnType("datetime");
            entity.Property(e => e.ExtOrderNo).HasMaxLength(255);
            entity.Property(e => e.IsReady).HasColumnName("isReady");
            entity.Property(e => e.IsSee).HasColumnName("isSee");
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.OriginalPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ReadyDate).HasColumnType("datetime");
            entity.Property(e => e.UserK).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<BillPrinter>(entity =>
        {
            entity.ToTable("BillPrinter");

            entity.Property(e => e.ComputerName).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PrinterName).HasMaxLength(150);
            entity.Property(e => e.UserName).HasMaxLength(150);
        });

        modelBuilder.Entity<BillWithHeader>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("BillWithHeader");

            entity.Property(e => e.CampaingGuid).HasMaxLength(50);
            entity.Property(e => e.Comment).HasMaxLength(250);
            entity.Property(e => e.DailyBillNumber).HasMaxLength(255);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ExtOrderNo).HasMaxLength(255);
            entity.Property(e => e.Fisno).HasMaxLength(100);
            entity.Property(e => e.IsReady).HasColumnName("isReady");
            entity.Property(e => e.IsSee).HasColumnName("isSee");
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ReadyDate).HasColumnType("datetime");
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.TableGroupId).HasMaxLength(255);
            entity.Property(e => e.TableNumber).HasMaxLength(250);
            entity.Property(e => e.UserK).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.VatRate).HasColumnType("decimal(4, 2)");
        });

        modelBuilder.Entity<BillWithHeaderOkc>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("BillWithHeaderOKC");

            entity.Property(e => e.CampaingGuid).HasMaxLength(50);
            entity.Property(e => e.Comment).HasMaxLength(250);
            entity.Property(e => e.DailyBillNumber).HasMaxLength(255);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ExtOrderNo).HasMaxLength(255);
            entity.Property(e => e.Fisno).HasMaxLength(100);
            entity.Property(e => e.IsReady).HasColumnName("isReady");
            entity.Property(e => e.IsSee).HasColumnName("isSee");
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ReadyDate).HasColumnType("datetime");
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.TableGroupId).HasMaxLength(255);
            entity.Property(e => e.TableNumber).HasMaxLength(250);
            entity.Property(e => e.UserK).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.VatRate).HasColumnType("decimal(4, 2)");
        });

        modelBuilder.Entity<Bom>(entity =>
        {
            entity.ToTable("Bom");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MaterialName).HasMaxLength(450);
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.StokId).HasColumnName("StokID");
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<BomOption>(entity =>
        {
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MaterialName).HasMaxLength(450);
            entity.Property(e => e.Menu).HasMaxLength(50);
            entity.Property(e => e.OptionsName).HasMaxLength(450);
            entity.Property(e => e.OptionsName2).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.StokId).HasColumnName("StokID");
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<CallLog>(entity =>
        {
            entity.ToTable("CallLog");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Line).HasMaxLength(250);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Server).HasMaxLength(50);
        });

        modelBuilder.Entity<CallLog2>(entity =>
        {
            entity.ToTable("CallLog2");

            entity.Property(e => e.AnswerTime).HasColumnType("datetime");
            entity.Property(e => e.CallId).HasMaxLength(50);
            entity.Property(e => e.CallTime).HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Line).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<CampaignDetail>(entity =>
        {
            entity.ToTable("CampaignDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FreeQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.SoldQuantity).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<CampaignHeader>(entity =>
        {
            entity.ToTable("CampaignHeader");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CampaignName).HasMaxLength(350);
            entity.Property(e => e.DaysOfWeek).HasMaxLength(20);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasMaxLength(5);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasMaxLength(5);
        });

        modelBuilder.Entity<CashierState>(entity =>
        {
            entity.ToTable("CashierState");

            entity.Property(e => e.Cash).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.CashierType).HasMaxLength(150);
            entity.Property(e => e.CreditCard).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Ticket).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.UserName).HasMaxLength(150);
        });

        modelBuilder.Entity<Choice1>(entity =>
        {
            entity.ToTable("Choice1");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Choice2>(entity =>
        {
            entity.ToTable("Choice2");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Collect>(entity =>
        {
            entity.ToTable("Collect");

            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Deliverer).HasMaxLength(50);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
            entity.Property(e => e.SubType).HasMaxLength(20);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<CollectPaid>(entity =>
        {
            entity.ToTable("CollectPaid");

            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Deliverer).HasMaxLength(50);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
            entity.Property(e => e.SubType).HasMaxLength(20);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Cost>(entity =>
        {
            entity.ToTable("Cost");

            entity.Property(e => e.BomCost).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.Cost1)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("Cost");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.ProductName).HasMaxLength(500);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer");

            entity.HasIndex(e => new { e.TaxNumber, e.CustomerName }, "IX_Customer");

            entity.Property(e => e.Address1).HasMaxLength(500);
            entity.Property(e => e.Address2).HasMaxLength(500);
            entity.Property(e => e.Adi).HasMaxLength(100);
            entity.Property(e => e.CardNo).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(150);
            entity.Property(e => e.CreditAllowance).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CustomerCode).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EfaturaUser).HasColumnName("EFaturaUser");
            entity.Property(e => e.Eposta)
                .HasMaxLength(150)
                .HasColumnName("EPosta");
            entity.Property(e => e.FullName).HasMaxLength(250);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Kdvtevkifatli)
                .HasDefaultValue(false)
                .HasColumnName("KDVTevkifatli");
            entity.Property(e => e.Passive).HasDefaultValue(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Soyadi).HasMaxLength(100);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.TaxOffice).HasMaxLength(50);
            entity.Property(e => e.TicariSicilNo).HasMaxLength(100);
        });

        modelBuilder.Entity<DebitPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DebitPayments");

            entity.ToTable("DebitPayment");

            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<DefaultOption>(entity =>
        {
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.ProductName).HasMaxLength(450);
        });

        modelBuilder.Entity<DeletedBill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DeletedBills");

            entity.ToTable("DeletedBill");

            entity.Property(e => e.Comment).HasMaxLength(450);
            entity.Property(e => e.DailyBillNumber).HasMaxLength(450);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DeleteDetails).HasMaxLength(450);
            entity.Property(e => e.DeleteReason).HasMaxLength(450);
            entity.Property(e => e.DeletingTime).HasColumnType("datetime");
            entity.Property(e => e.DeletingUserName).HasMaxLength(50);
            entity.Property(e => e.Fisno)
                .HasMaxLength(100)
                .HasDefaultValue("1");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.OriginalPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.TableNumber).HasMaxLength(250);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Deliverer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Deliverers");

            entity.ToTable("Deliverer");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<DigiPan>(entity =>
        {
            entity.ToTable("DigiPan");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(50);
        });

        modelBuilder.Entity<DirectTransaction>(entity =>
        {
            entity.ToTable("DirectTransaction");

            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<DiscountDetail>(entity =>
        {
            entity.ToTable("DiscountDetail");

            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.ProductGroup).HasMaxLength(254);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 4)");
        });

        modelBuilder.Entity<EfaturaDurumKodlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_EFaturaDurumAciklama");

            entity.ToTable("EFaturaDurumKodlari");

            entity.Property(e => e.Durum).HasMaxLength(500);
        });

        modelBuilder.Entity<FormalBillPrinter>(entity =>
        {
            entity.ToTable("FormalBillPrinter");

            entity.Property(e => e.ComputerName).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PrinterName).HasMaxLength(150);
            entity.Property(e => e.UserName).HasMaxLength(150);
        });

        modelBuilder.Entity<MarchingProduct>(entity =>
        {
            entity.Property(e => e.ProductId).HasDefaultValue(0);
            entity.Property(e => e.ProductName).HasMaxLength(250);
        });

        modelBuilder.Entity<MaterialTransaction>(entity =>
        {
            entity.ToTable("MaterialTransaction");

            entity.HasIndex(e => new { e.Branch, e.Department }, "IX_MaterialTransaction_Branch_Department");

            entity.HasIndex(e => e.Date, "IX_MaterialTransaction_Date");

            entity.HasIndex(e => e.MaterialName, "IX_MaterialTransaction_MaterialName");

            entity.HasIndex(e => e.TransactionType, "IX_MaterialTransaction_TransactionType");

            entity.Property(e => e.Branch).HasMaxLength(20);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Department).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MaterialName).HasMaxLength(250);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.ToTable("Menu");

            entity.HasIndex(e => e.MenuName, "IX_Menu").IsUnique();

            entity.HasIndex(e => e.Active, "IX_Menu_1");

            entity.Property(e => e.Aktarildi).HasDefaultValue(false);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<MenuProduct>(entity =>
        {
            entity.ToTable("MenuProduct");

            entity.HasIndex(e => new { e.MenuId, e.ProductName }, "IX_MenuProduct").IsUnique();

            entity.HasIndex(e => e.MenuId, "IX_MenuProduct_1");

            entity.Property(e => e.Aktarildi).HasDefaultValue(false);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(50);
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<OptionCat>(entity =>
        {
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(254);
        });

        modelBuilder.Entity<OptionsEqu>(entity =>
        {
            entity.ToTable("OptionsEqu");

            entity.Property(e => e.EquProduct).HasMaxLength(450);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MenuFiyat).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.MenuYuzde).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.Miktar).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.Options).HasMaxLength(450);
            entity.Property(e => e.ProductName).HasMaxLength(450);
        });

        modelBuilder.Entity<OrderHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Orders");

            entity.ToTable("OrderHeader");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<OrderPrinter>(entity =>
        {
            entity.ToTable("OrderPrinter");

            entity.Property(e => e.ComputerName).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.MachineName).HasMaxLength(150);
            entity.Property(e => e.PrinterName).HasMaxLength(150);
            entity.Property(e => e.ProductGroup).HasMaxLength(150);
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.TablePrefix).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(150);
        });

        modelBuilder.Entity<PaidBill>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PaidBill");

            entity.Property(e => e.CampaingGuid).HasMaxLength(50);
            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Comment).HasMaxLength(250);
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Debit).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountReason).HasMaxLength(100);
            entity.Property(e => e.ExtOrderNo).HasMaxLength(255);
            entity.Property(e => e.IsReady).HasColumnName("isReady");
            entity.Property(e => e.IsSee).HasColumnName("isSee");
            entity.Property(e => e.MenuName).HasMaxLength(50);
            entity.Property(e => e.OnlinePayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Options).HasMaxLength(1024);
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ReadyDate).HasColumnType("datetime");
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
            entity.Property(e => e.TableNo).HasMaxLength(250);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserK).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.VatRate).HasColumnType("decimal(4, 2)");
        });

        modelBuilder.Entity<PaketEntegrasyon>(entity =>
        {
            entity.ToTable("PaketEntegrasyon");

            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.GtRestaurantId)
                .HasMaxLength(100)
                .HasColumnName("gtRestaurantId");
            entity.Property(e => e.GtRestaurantSk)
                .HasMaxLength(100)
                .HasColumnName("gtRestaurantSK");
            entity.Property(e => e.GtToken)
                .HasMaxLength(1000)
                .HasColumnName("gtToken");
            entity.Property(e => e.GtTokenEndTime)
                .HasColumnType("datetime")
                .HasColumnName("gtTokenEndTime");
            entity.Property(e => e.MyApiKey)
                .HasMaxLength(100)
                .HasColumnName("myApiKey");
            entity.Property(e => e.MyRestoranId)
                .HasMaxLength(100)
                .HasColumnName("myRestoranID");
            entity.Property(e => e.MyToken)
                .HasMaxLength(1000)
                .HasColumnName("myToken");
            entity.Property(e => e.MyTokenEndTime)
                .HasColumnType("datetime")
                .HasColumnName("myTokenEndTime");
            entity.Property(e => e.MyZincirId)
                .HasMaxLength(100)
                .HasColumnName("myZincirID");
            entity.Property(e => e.PaketType).HasColumnName("paket_type");
            entity.Property(e => e.TyApiKey)
                .HasMaxLength(100)
                .HasColumnName("tyApiKey");
            entity.Property(e => e.TyApiSecret)
                .HasMaxLength(100)
                .HasColumnName("tyApiSecret");
            entity.Property(e => e.TyEposta)
                .HasMaxLength(100)
                .HasColumnName("tyEPosta");
            entity.Property(e => e.TySaticiId)
                .HasMaxLength(100)
                .HasColumnName("tySaticiID");
            entity.Property(e => e.TySubeKodu)
                .HasMaxLength(100)
                .HasColumnName("tySubeKodu");
            entity.Property(e => e.TyToken)
                .HasMaxLength(1000)
                .HasColumnName("tyToken");
            entity.Property(e => e.TyTokenEndTime)
                .HasColumnType("datetime")
                .HasColumnName("tyTokenEndTime");
            entity.Property(e => e.YsCatalogName)
                .HasMaxLength(100)
                .HasColumnName("YS_CatalogName");
            entity.Property(e => e.YsCategoryName)
                .HasMaxLength(100)
                .HasColumnName("YS_CategoryName");
            entity.Property(e => e.YsChaincode)
                .HasMaxLength(100)
                .HasColumnName("YS_Chaincode");
            entity.Property(e => e.YsId)
                .HasMaxLength(200)
                .HasColumnName("YS_Id");
            entity.Property(e => e.YsPassword)
                .HasMaxLength(100)
                .HasColumnName("YS_Password");
            entity.Property(e => e.YsRemoteId)
                .HasMaxLength(10)
                .HasColumnName("YS_RemoteId");
            entity.Property(e => e.YsRestaurantId)
                .HasMaxLength(100)
                .HasColumnName("YS_RestaurantId");
            entity.Property(e => e.YsToken)
                .HasMaxLength(1000)
                .HasColumnName("YS_Token");
            entity.Property(e => e.YsTokenEndTime)
                .HasColumnType("datetime")
                .HasColumnName("YS_TokenEndTime");
            entity.Property(e => e.YsUserName)
                .HasMaxLength(100)
                .HasColumnName("YS_UserName");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Payments");

            entity.ToTable("Payment", tb => tb.HasTrigger("tg_InsertPayment"));

            entity.HasIndex(e => e.HeaderId, "IX_Payment_HeaderId");

            entity.HasIndex(e => e.PaymentTime, "IX_Payment_PaymentTime");

            entity.Property(e => e.BelgeNotu).HasMaxLength(150);
            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Debit).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Deliverer).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountReason).HasMaxLength(100);
            entity.Property(e => e.EfaturaTipi).HasColumnName("EFaturaTipi");
            entity.Property(e => e.Ettn)
                .HasMaxLength(60)
                .HasColumnName("ETTN");
            entity.Property(e => e.FastSale).HasDefaultValue(true);
            entity.Property(e => e.IdentificationNo).HasMaxLength(255);
            entity.Property(e => e.InvoiceNo).HasMaxLength(255);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.KdvtevkifatTutari)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("KDVTevkifatTutari");
            entity.Property(e => e.KrediKarti).HasMaxLength(50);
            entity.Property(e => e.OnlineOdeme).HasMaxLength(50);
            entity.Property(e => e.OnlinePayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.PersonName).HasMaxLength(255);
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
            entity.Property(e => e.TableNo).HasMaxLength(250);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.YemekKarti).HasMaxLength(50);
        });

        modelBuilder.Entity<PaymentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PaymentDetail");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.KasaAciklama).HasMaxLength(150);
            entity.Property(e => e.PaymentDetail1)
                .HasMaxLength(50)
                .HasColumnName("PaymentDetail");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PaymentMethod");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PaymentGroup).HasMaxLength(50);
            entity.Property(e => e.PaymentMethod1)
                .HasMaxLength(50)
                .HasColumnName("PaymentMethod");
        });

        modelBuilder.Entity<PaymentPaket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PaymentsPaket");

            entity.ToTable("PaymentPaket");

            entity.Property(e => e.BelgeNotu).HasMaxLength(150);
            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Debit).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Deliverer).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountReason).HasMaxLength(100);
            entity.Property(e => e.EfaturaTipi).HasColumnName("EFaturaTipi");
            entity.Property(e => e.Ettn)
                .HasMaxLength(60)
                .HasColumnName("ETTN");
            entity.Property(e => e.IdentificationNo).HasMaxLength(255);
            entity.Property(e => e.InvoiceNo).HasMaxLength(255);
            entity.Property(e => e.KrediKarti).HasMaxLength(50);
            entity.Property(e => e.OnlineOdeme).HasMaxLength(50);
            entity.Property(e => e.OnlinePayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.PersonName).HasMaxLength(255);
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(50);
            entity.Property(e => e.TableNo).HasMaxLength(250);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.YemekKarti).HasMaxLength(50);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => new { e.UserName, e.PermissionName }).HasName("PK_Permissions");

            entity.ToTable("Permission");

            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.PermissionName).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.PermissionValue).HasMaxLength(50);
        });

        modelBuilder.Entity<PhoneDictionary>(entity =>
        {
            entity.HasKey(e => e.PhoneNumber);

            entity.ToTable("PhoneDictionary");

            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Directions).HasMaxLength(500);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.LastCustomerNote).HasMaxLength(500);
            entity.Property(e => e.LastPaymentNote).HasMaxLength(500);
            entity.Property(e => e.OrderOwnerName).HasMaxLength(50);
        });

        modelBuilder.Entity<PhoneOrderHeader>(entity =>
        {
            entity.ToTable("PhoneOrderHeader", tb => tb.HasTrigger("delivererassignmentstate"));

            entity.HasIndex(e => e.HeaderId, "PhoneOrderHeader_HeaderIdIDX");

            entity.Property(e => e.Address).HasMaxLength(1000);
            entity.Property(e => e.AssignDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedByUserName).HasMaxLength(50);
            entity.Property(e => e.CreationTime).HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.CustomerNote).HasMaxLength(450);
            entity.Property(e => e.Deliverer).HasMaxLength(50);
            entity.Property(e => e.Directions).HasMaxLength(450);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.OrderNo).HasMaxLength(50);
            entity.Property(e => e.OrderOwnerName).HasMaxLength(50);
            entity.Property(e => e.OrderStatus).HasMaxLength(50);
            entity.Property(e => e.PaketEntegrasyonId).HasDefaultValue(0);
            entity.Property(e => e.PaymentDetail).HasMaxLength(450);
            entity.Property(e => e.PaymentNote).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.TrackingNumber).HasMaxLength(150);
        });

        modelBuilder.Entity<PhoneOrderJson>(entity =>
        {
            entity.ToTable("PhoneOrderJson");

            entity.HasIndex(e => e.TrackingNumber, "IX_PhoneOrderJson");

            entity.HasIndex(e => e.Token, "IX_PhoneOrderJson_1");

            entity.Property(e => e.Json).HasColumnType("text");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("_Message");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("_Time");
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TrackingNumber).HasMaxLength(150);
        });

        modelBuilder.Entity<PrePayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PrePayments");

            entity.ToTable("PrePayment");

            entity.Property(e => e.CashPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreditPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.KdvtevkifatTutari)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("KDVTevkifatTutari");
            entity.Property(e => e.KrediKarti).HasMaxLength(500);
            entity.Property(e => e.Online).HasMaxLength(500);
            entity.Property(e => e.OnlinePayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentTime).HasColumnType("datetime");
            entity.Property(e => e.PaymentType).HasMaxLength(50);
            entity.Property(e => e.TableNumber).HasMaxLength(250);
            entity.Property(e => e.TicketPayment).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserName).HasMaxLength(500);
            entity.Property(e => e.YemekKarti).HasMaxLength(500);
        });

        modelBuilder.Entity<PrintJob>(entity =>
        {
            entity.ToTable("PrintJob");

            entity.Property(e => e.ComputerName).HasMaxLength(50);
            entity.Property(e => e.Data).HasMaxLength(500);
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Products");

            entity.ToTable("Product");

            entity.Property(e => e.Favorites).HasMaxLength(255);
            entity.Property(e => e.InvoiceName).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.IstisnaKodu).HasMaxLength(3);
            entity.Property(e => e.Order).HasMaxLength(50);
            entity.Property(e => e.OzelMatrahKodu).HasMaxLength(3);
            entity.Property(e => e.Plu).HasMaxLength(50);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductGroup).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(450);
            entity.Property(e => e.ProductType).HasMaxLength(50);
            entity.Property(e => e.StockCode).HasMaxLength(20);
            entity.Property(e => e.VatRate).HasColumnType("decimal(4, 2)");
        });

        modelBuilder.Entity<ProductEqu>(entity =>
        {
            entity.ToTable("ProductEqu");

            entity.Property(e => e.Choice1Id).HasColumnName("choice1Id");
            entity.Property(e => e.Choice2Id).HasColumnName("choice2Id");
            entity.Property(e => e.EquProductName).HasMaxLength(450);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Multiplier).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(450);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductI__3214EC079E3AD611");

            entity.ToTable("ProductImage");

            entity.Property(e => e.Calory).HasMaxLength(50);
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Menu).HasMaxLength(50);
            entity.Property(e => e.ProductDefinition).HasMaxLength(2000);
            entity.Property(e => e.ProductGroup).HasMaxLength(50);
            entity.Property(e => e.ServiceTime).HasMaxLength(50);
        });

        modelBuilder.Entity<ProductLongName>(entity =>
        {
            entity.ToTable("ProductLongName");

            entity.Property(e => e.ProductName).HasMaxLength(250);
        });

        modelBuilder.Entity<ProductTemplate>(entity =>
        {
            entity.ToTable("ProductTemplate");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<ProductTemplateAutomation>(entity =>
        {
            entity.ToTable("ProductTemplateAutomation");

            entity.Property(e => e.DateEnd).HasColumnType("datetime");
            entity.Property(e => e.DateStart).HasColumnType("datetime");
            entity.Property(e => e.DaysOfWeek).HasMaxLength(100);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.TemplateName).HasMaxLength(100);
            entity.Property(e => e.TimeEnd).HasMaxLength(100);
            entity.Property(e => e.TimeStart).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductTemplatePrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProductTemplatePrices");

            entity.ToTable("ProductTemplatePrice");

            entity.HasIndex(e => e.TemplateId, "IX_ProductTemplatePrice_TemplateId");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Production>(entity =>
        {
            entity.ToTable("Production");

            entity.Property(e => e.Branch).HasMaxLength(20);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Department).HasMaxLength(20);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ProductName).HasMaxLength(250);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
        });

        modelBuilder.Entity<ReasonList>(entity =>
        {
            entity.ToTable("ReasonList");

            entity.Property(e => e.Description).HasMaxLength(20);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Key).HasMaxLength(20);
        });

        modelBuilder.Entity<ReservationList>(entity =>
        {
            entity.ToTable("ReservationList");

            entity.Property(e => e.Confirm).HasDefaultValue(true);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasMaxLength(10);
            entity.Property(e => e.IsSee).HasColumnName("isSee");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.ReservationName).HasMaxLength(100);
            entity.Property(e => e.ReservationName2).HasMaxLength(100);
            entity.Property(e => e.ReservationPhone).HasMaxLength(20);
            entity.Property(e => e.ReservationPhone2).HasMaxLength(20);
            entity.Property(e => e.StartTime).HasMaxLength(10);
            entity.Property(e => e.TableNumber).HasMaxLength(50);
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("_Time");
        });

        modelBuilder.Entity<SayimBaslik>(entity =>
        {
            entity.ToTable("SayimBaslik");

            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.SayimTarihi).HasColumnType("datetime");
        });

        modelBuilder.Entity<SayimDetay>(entity =>
        {
            entity.ToTable("SayimDetay");

            entity.Property(e => e.Alisfiyati)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("ALISFIYATI");
            entity.Property(e => e.Birimid).HasColumnName("BIRIMID");
            entity.Property(e => e.EnvanterOrg)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("ENVANTER_ORG");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Kaynak).HasColumnName("KAYNAK");
            entity.Property(e => e.Maliyet)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("MALIYET");
            entity.Property(e => e.Sayim)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("SAYIM");
            entity.Property(e => e.Stokind).HasColumnName("STOKIND");
            entity.Property(e => e.Zayi)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("ZAYI");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shift__3214EC0782B9B625");

            entity.ToTable("Shift");

            entity.Property(e => e.Ends).HasColumnType("datetime");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Starts).HasColumnType("datetime");
        });

        modelBuilder.Entity<StockLevel>(entity =>
        {
            entity.HasKey(e => e.ProductName).HasName("PK_StockLevels");

            entity.ToTable("StockLevel");

            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.CriticalLevel).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Inventory).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
        });

        modelBuilder.Entity<SubeLink>(entity =>
        {
            entity.Property(e => e.FiyatSablonu).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Kullanici).HasMaxLength(50);
            entity.Property(e => e.Parola).HasMaxLength(50);
            entity.Property(e => e.SubeAdi).HasMaxLength(50);
            entity.Property(e => e.Sunucu).HasMaxLength(50);
            entity.Property(e => e.Veritabani).HasMaxLength(50);
        });

        modelBuilder.Entity<SyncTable>(entity =>
        {
            entity.ToTable("SyncTable");
        });

        modelBuilder.Entity<TableGroup>(entity =>
        {
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(1024);
            entity.Property(e => e.Prefix).HasMaxLength(1024);
            entity.Property(e => e.Settings).HasMaxLength(500);
        });

        modelBuilder.Entity<TableLock>(entity =>
        {
            entity.HasKey(e => e.TableNumber);

            entity.ToTable("TableLock");

            entity.Property(e => e.TableNumber).HasMaxLength(255);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tblpbipuanyetki>(entity =>
        {
            entity.ToTable("TBLPBIPUANYETKI");

            entity.Property(e => e.Aciklama)
                .HasMaxLength(50)
                .HasColumnName("ACIKLAMA");
            entity.Property(e => e.Userno)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("USERNO");
            entity.Property(e => e.Yetki)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("YETKI");
        });

        modelBuilder.Entity<Tblpospuan>(entity =>
        {
            entity.ToTable("TBLPOSPUAN");

            entity.Property(e => e.Aktarildi).HasColumnName("AKTARILDI");
            entity.Property(e => e.Aktarildi1).HasColumnName("Aktarildi");
            entity.Property(e => e.Belgeind).HasColumnName("BELGEIND");
            entity.Property(e => e.Belgeno)
                .HasMaxLength(10)
                .HasColumnName("BELGENO");
            entity.Property(e => e.Belgetipi).HasColumnName("BELGETIPI");
            entity.Property(e => e.Cariind).HasColumnName("CARIIND");
            entity.Property(e => e.Puan)
                .HasColumnType("decimal(28, 8)")
                .HasColumnName("PUAN");
            entity.Property(e => e.Tarih)
                .HasColumnType("datetime")
                .HasColumnName("TARIH");
        });

        modelBuilder.Entity<Tblpospuanaciklama>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TBLPOSPUANACIKLAMA");

            entity.Property(e => e.Aciklama)
                .HasMaxLength(150)
                .HasColumnName("ACIKLAMA");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Pind).HasColumnName("PIND");
        });

        modelBuilder.Entity<TemplateOverride>(entity =>
        {
            entity.Property(e => e.CustomerCategory).HasMaxLength(150);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.TableGroup).HasMaxLength(150);
            entity.Property(e => e.TemplateName).HasMaxLength(150);
        });

        modelBuilder.Entity<TeraPosDef>(entity =>
        {
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(254);
        });

        modelBuilder.Entity<TriggerTable>(entity =>
        {
            entity.ToTable("TriggerTable");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Command).HasMaxLength(20);
            entity.Property(e => e.TableColumn).HasMaxLength(100);
            entity.Property(e => e.TableId).HasMaxLength(100);
            entity.Property(e => e.TableName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Users");

            entity.ToTable("User");

            entity.HasIndex(e => e.UserName, "IX_User").IsUnique();

            entity.Property(e => e.Branch).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(20);
            entity.Property(e => e.DiscAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DrawerPort).HasMaxLength(10);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.ProductTemplate).HasMaxLength(50);
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .HasDefaultValue("User");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.ToTable("UserLogin");

            entity.Property(e => e.Aktarildi).HasDefaultValue(false);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Login)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Logout).HasColumnType("datetime");
            entity.Property(e => e.MachineName).HasMaxLength(150);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserProduct>(entity =>
        {
            entity.HasIndex(e => new { e.UserName, e.ProductId }, "IX_UserProducts").IsUnique();

            entity.HasIndex(e => e.ProductId, "IX_UserProducts_1");

            entity.HasIndex(e => e.UserName, "IX_UserProducts_2");

            entity.Property(e => e.Aktarildi).HasDefaultValue(false);
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<Value>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PK_Values");

            entity.ToTable("Value");

            entity.HasIndex(e => e.Value1, "IX_VALUE_VALUE");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Value1)
                .HasMaxLength(4000)
                .HasColumnName("value");
        });

        modelBuilder.Entity<VerInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("VerInfo");

            entity.Property(e => e.Content)
                .HasColumnType("ntext")
                .HasColumnName("_Content");
            entity.Property(e => e.File)
                .HasColumnType("image")
                .HasColumnName("_File");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
        });

        modelBuilder.Entity<Waste>(entity =>
        {
            entity.ToTable("Waste");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<WeighingProduct>(entity =>
        {
            entity.Property(e => e.IsSynced).HasDefaultValue(false);
            entity.Property(e => e.IsUpdated).HasDefaultValue(false);
            entity.Property(e => e.ProductName).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
