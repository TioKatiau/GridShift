from pathlib import Path
from html import escape
from reportlab.pdfgen import canvas
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, KeepTogether
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib import colors
from reportlab.lib.enums import TA_LEFT
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

ROOT = Path(__file__).resolve().parents[1]
pdfmetrics.registerFont(TTFont('ArialGS', 'C:/Windows/Fonts/arial.ttf'))
pdfmetrics.registerFont(TTFont('ArialGSBold', 'C:/Windows/Fonts/arialbd.ttf'))
styles = {
    'body': ParagraphStyle('body', fontName='ArialGS', fontSize=10.2, leading=15, textColor=colors.HexColor('#20354b'), spaceAfter=9),
    'h1': ParagraphStyle('h1', fontName='ArialGSBold', fontSize=25, leading=30, textColor=colors.HexColor('#112a43'), spaceAfter=18),
    'h2': ParagraphStyle('h2', fontName='ArialGSBold', fontSize=14, leading=19, textColor=colors.HexColor('#123c62'), spaceBefore=13, spaceAfter=9, keepWithNext=True),
}

def frame(c, doc):
    c.setFillColor(colors.HexColor('#10273e'))
    c.rect(0, 803, 595.28, 39, fill=1, stroke=0)
    c.setFillColor(colors.HexColor('#ffce36'))
    c.setFont('ArialGSBold', 11)
    c.drawString(45, 817, 'GS  /  GRIDSHIFT')
    c.setFillColor(colors.white)
    c.setFont('ArialGS', 8)
    c.drawRightString(550, 818, 'ENTREGA ACADÊMICA 2026  ·  V26.0')
    c.setStrokeColor(colors.HexColor('#e0e6ec'))
    c.line(45, 43, 550, 43)
    c.setFillColor(colors.HexColor('#637387'))
    c.setFont('ArialGS', 8)
    c.drawString(45, 29, 'GridShift · 14 de novembro de 2026')
    c.drawRightString(550, 29, str(doc.page))

for name in ('GDD', 'Testes_Bugs', 'Devlog'):
    text = (ROOT / (name + '.md')).read_text(encoding='utf-8-sig')
    flow = []
    for block in text.split('\n\n'):
        block = block.strip()
        if not block: continue
        key = 'h2' if block.startswith('## ') else 'h1' if block.startswith('# ') else 'body'
        if key == 'h1': block = block[2:]
        if key == 'h2': block = block[3:]
        flow.append(Paragraph(escape(block).replace('\n', '<br/>'), styles[key]))
    doc = SimpleDocTemplate(str(ROOT / (name + '.pdf')), pagesize=(595.28,841.89), rightMargin=45,leftMargin=45,topMargin=65,bottomMargin=59,title='GridShift — '+name,author='Equipe GridShift')
    doc.build(flow,onFirstPage=frame,onLaterPages=frame)
    print(name + '.pdf')
