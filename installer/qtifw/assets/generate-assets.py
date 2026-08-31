#!/usr/bin/env python3
"""Generates installer assets for Qt Installer Framework (QtIFW).

Reuses the visual identity from the original NSIS installer (installer/assets/generate-assets.py):
same color palette, typography, and Şefim branding.

Outputs PNG/ICO/ICNS formats expected by config.xml:
  installer.png  256x256   window icon
  installer.ico  multi     Windows icon (16/32/48/256)
  installer.icns multi     macOS icon
  logo.png       various   wizard/welcome logo image

Run from the repository root:
    python3 installer/qtifw/assets/generate-assets.py
"""

from __future__ import annotations

import os
from io import BytesIO

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))

# Color palette from original NSIS installer
DEEP = (10, 26, 47)
ACCENT = (31, 111, 235)
LIGHT = (236, 242, 250)
WHITE = (255, 255, 255)

FONT_DIR = "/usr/share/fonts/truetype/dejavu"
FONT_BOLD = os.path.join(FONT_DIR, "DejaVuSans-Bold.ttf")
FONT_REGULAR = os.path.join(FONT_DIR, "DejaVuSans.ttf")


def font(path: str, size: int) -> ImageFont.FreeTypeFont:
    try:
        return ImageFont.truetype(path, size)
    except OSError:
        # Fallback to default if fonts not available
        return ImageFont.load_default()


def vertical_gradient(size: tuple[int, int], top: tuple[int, int, int],
                      bottom: tuple[int, int, int]) -> Image.Image:
    """Create a vertical gradient from top to bottom color."""
    width, height = size
    image = Image.new("RGB", size, top)
    draw = ImageDraw.Draw(image)
    for y in range(height):
        ratio = y / max(height - 1, 1)
        draw.line(
            [(0, y), (width, y)],
            fill=tuple(int(top[i] + (bottom[i] - top[i]) * ratio) for i in range(3)),
        )
    return image


def diagonal_sheen(image: Image.Image, alpha: int = 18) -> Image.Image:
    """Add a subtle diagonal shine/gloss overlay to an image."""
    overlay = Image.new("RGBA", image.size, (255, 255, 255, 0))
    draw = ImageDraw.Draw(overlay)
    width, height = image.size
    draw.polygon(
        [(0, height), (width, height - int(height * 0.45)), (width, height), (0, height)],
        fill=(255, 255, 255, alpha),
    )
    draw.polygon(
        [(0, 0), (int(width * 0.75), 0), (0, int(height * 0.35))],
        fill=(255, 255, 255, alpha // 2),
    )
    return Image.alpha_composite(image.convert("RGBA"), overlay).convert("RGB")


def mark(draw: ImageDraw.ImageDraw, cx: int, cy: int, radius: int, ring: int) -> None:
    """Draw the Şefim ring mark: ring with Ş initial."""
    draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=ACCENT)
    inner = radius - ring
    draw.ellipse([cx - inner, cy - inner, cx + inner, cy + inner], fill=DEEP)
    draw.text((cx, cy), "Ş", font=font(FONT_BOLD, int(inner * 2.0)), fill=WHITE, anchor="mm")


def build_icon(size: int) -> Image.Image:
    """Build a square icon at the given size with the Şefim brand mark."""
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    # Rounded rectangle background
    radius = int(size * 0.19)  # ~48px for 256px
    draw.rounded_rectangle([0, 0, size - 1, size - 1], radius=radius, fill=DEEP)

    # Brand mark (Ş in circle)
    mark_radius = int(size * 0.31)  # ~78px for 256px
    ring_width = int(size * 0.086)  # ~22px for 256px
    mark(draw, size // 2, int(size * 0.45), mark_radius, ring_width)

    # MCP text below
    font_size = int(size * 0.156)  # ~40px for 256px
    draw.text((size // 2, int(size * 0.84)), "MCP", font=font(FONT_BOLD, font_size), fill=WHITE, anchor="mm")

    return image


def build_icon_rgb(size: int) -> Image.Image:
    """Build icon and convert to RGB (no alpha channel)."""
    icon = build_icon(size)
    if icon.mode == "RGBA":
        background = Image.new("RGB", icon.size, DEEP)
        background.paste(icon, mask=icon.split()[3])
        return background
    return icon.convert("RGB")


def build_installer_png() -> None:
    """Build installer.png: 256x256 PNG window icon."""
    icon = build_icon(256).convert("RGB")
    icon.save(os.path.join(HERE, "installer.png"), "PNG")
    print(f"  installer.png (256x256)")


def build_installer_ico() -> None:
    """Build installer.ico: multi-size Windows icon (16, 32, 48, 256)."""
    sizes = [16, 32, 48, 256]
    images = [build_icon_rgb(size) for size in sizes]

    # Save as ICO with all sizes
    images[0].save(
        os.path.join(HERE, "installer.ico"),
        format="ICO",
        sizes=[(size, size) for size in sizes],
        append_images=images[1:],
    )
    print(f"  installer.ico (multi-size: {', '.join(str(s) for s in sizes)})")


def build_installer_icns() -> None:
    """Build installer.icns: macOS icon.

    Pillow >= 8.3.0 can export ICNS directly. If unavailable, we generate PNGs
    that can be assembled with iconutil on macOS.
    """
    # macOS icon sizes: 16, 32, 64, 128, 256, 512, 1024
    sizes = [16, 32, 64, 128, 256, 512, 1024]
    icon_1024 = build_icon(1024).convert("RGB")

    try:
        # Try to save as ICNS directly (Pillow >= 8.3.0)
        images = [build_icon_rgb(size) for size in sizes]
        images[0].save(
            os.path.join(HERE, "installer.icns"),
            format="ICNS",
            sizes=[(size, size) for size in sizes],
            append_images=images[1:],
        )
        print(f"  installer.icns (macOS, multi-size)")
    except (OSError, KeyError):
        # Fallback: save as 1024x1024 PNG which can be converted on macOS with iconutil
        icon_1024.save(os.path.join(HERE, "installer_1024.png"), "PNG")
        print(f"  installer.icns (ICNS export not available; saved 1024x1024 PNG instead)")
        print(f"    → On macOS, run: iconutil -c icns -o installer.icns installer_1024.png")


def build_logo_png() -> None:
    """Build logo.png: wizard/welcome logo image.

    Reuses the welcome image design from the original NSIS installer:
    vertical gradient background with the Şefim mark and product text.
    """
    # Wizard logo size (common for Qt installers)
    width, height = 164, 314

    image = diagonal_sheen(vertical_gradient((width, height), DEEP, (18, 62, 112)))
    draw = ImageDraw.Draw(image)

    # Brand mark (Ş in circle)
    mark(draw, width // 2, int(height * 0.29), 34, 9)

    # Text layout
    title = font(FONT_BOLD, 21)
    subtitle = font(FONT_REGULAR, 12)
    footer = font(FONT_REGULAR, 10)

    draw.text((width // 2, int(height * 0.50)), "ŞEFİM", font=title, fill=WHITE, anchor="mm")
    draw.text((width // 2, int(height * 0.57)), "MCP SERVER", font=subtitle, fill=LIGHT, anchor="mm")
    draw.line([(width * 0.25, int(height * 0.64)), (width * 0.75, int(height * 0.64))], fill=ACCENT, width=2)
    draw.text((width // 2, int(height * 0.70)), "Yapay zekâ ile", font=footer, fill=LIGHT, anchor="mm")
    draw.text((width // 2, int(height * 0.75)), "Şefim POS entegrasyonu", font=footer, fill=LIGHT, anchor="mm")
    draw.text((width // 2, int(height * 0.93)), "OZFİLİZ YAZILIM", font=footer, fill=(150, 180, 220), anchor="mm")

    image.save(os.path.join(HERE, "logo.png"), "PNG")
    print(f"  logo.png (164x314, wizard banner)")


if __name__ == "__main__":
    print("Generating QtIFW installer assets...")
    build_logo_png()
    build_installer_png()
    build_installer_ico()
    build_installer_icns()
    print("\nAssets generated successfully in:", HERE)
