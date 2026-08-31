#!/usr/bin/env python3
"""Generates the bitmap and icon assets used by installer/sefim-mcp.nsi.

MUI2 requires 24-bit BMP files with fixed sizes:
  welcome.bmp  164x314   welcome and finish page side banner
  header.bmp   150x57    inner page header

Run from the repository root:
    python3 installer/assets/generate-assets.py
"""

from __future__ import annotations

import os

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))

DEEP = (10, 26, 47)
ACCENT = (31, 111, 235)
LIGHT = (236, 242, 250)
WHITE = (255, 255, 255)

FONT_DIR = "/usr/share/fonts/truetype/dejavu"
FONT_BOLD = os.path.join(FONT_DIR, "DejaVuSans-Bold.ttf")
FONT_REGULAR = os.path.join(FONT_DIR, "DejaVuSans.ttf")


def font(path: str, size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(path, size)


def vertical_gradient(size: tuple[int, int], top: tuple[int, int, int],
                      bottom: tuple[int, int, int]) -> Image.Image:
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
    """Ring with the Şefim initial, readable down to 16 px."""
    draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=ACCENT)
    inner = radius - ring
    draw.ellipse([cx - inner, cy - inner, cx + inner, cy + inner], fill=DEEP)
    draw.text((cx, cy), "Ş", font=font(FONT_BOLD, int(inner * 2.0)), fill=WHITE, anchor="mm")


def build_welcome() -> None:
    image = diagonal_sheen(vertical_gradient((164, 314), DEEP, (18, 62, 112)))
    draw = ImageDraw.Draw(image)

    mark(draw, 82, 92, 34, 9)

    title = font(FONT_BOLD, 21)
    subtitle = font(FONT_REGULAR, 12)
    footer = font(FONT_REGULAR, 10)

    draw.text((82, 156), "ŞEFİM", font=title, fill=WHITE, anchor="mm")
    draw.text((82, 180), "MCP SERVER", font=subtitle, fill=LIGHT, anchor="mm")
    draw.line([(42, 200), (122, 200)], fill=ACCENT, width=2)
    draw.text((82, 220), "Yapay zekâ ile", font=footer, fill=LIGHT, anchor="mm")
    draw.text((82, 234), "Şefim POS entegrasyonu", font=footer, fill=LIGHT, anchor="mm")
    draw.text((82, 292), "OZFİLİZ YAZILIM", font=footer, fill=(150, 180, 220), anchor="mm")

    image.save(os.path.join(HERE, "welcome.bmp"), "BMP")


def build_header() -> None:
    image = Image.new("RGB", (150, 57), WHITE)
    draw = ImageDraw.Draw(image)
    draw.rectangle([0, 0, 5, 57], fill=ACCENT)

    mark(draw, 28, 28, 15, 4)

    draw.text((50, 20), "ŞEFİM MCP", font=font(FONT_BOLD, 12), fill=DEEP, anchor="lm")
    draw.text((50, 36), "OZFİLİZ YAZILIM", font=font(FONT_REGULAR, 8), fill=(90, 110, 135), anchor="lm")

    image.save(os.path.join(HERE, "header.bmp"), "BMP")


def build_icon(name: str, uninstall: bool) -> None:
    base = 256
    image = Image.new("RGBA", (base, base), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle([0, 0, base - 1, base - 1], radius=48, fill=DEEP)
    mark(draw, base // 2, base // 2 - 10, 78, 22)

    if uninstall:
        draw.rectangle([base // 2 - 60, base // 2 - 22, base // 2 + 60, base // 2 + 2],
                       fill=(214, 72, 72))

    draw.text((base // 2, base - 42), "MCP", font=font(FONT_BOLD, 40), fill=WHITE, anchor="mm")
    image.save(os.path.join(HERE, name), sizes=[(256, 256), (64, 64), (48, 48), (32, 32), (16, 16)])


if __name__ == "__main__":
    build_welcome()
    build_header()
    build_icon("install.ico", uninstall=False)
    build_icon("uninstall.ico", uninstall=True)
    print("assets written to", HERE)
