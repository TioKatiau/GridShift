$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$basePath=Split-Path $PSScriptRoot -Parent
$projectPath=Split-Path (Split-Path $basePath -Parent) -Parent
$artPath=Join-Path $basePath 'arte'
New-Item -ItemType Directory -Path $artPath -Force | Out-Null
$logoPath=Join-Path $artPath 'GS_Original.png'
if(-not (Test-Path -LiteralPath $logoPath)){Copy-Item -LiteralPath (Join-Path $projectPath 'Assets\UI\GridShift\01_Logos\GS_Simbolo_Transparente.png') -Destination $logoPath}
$logo=[System.Drawing.Image]::FromFile($logoPath)
function Text($g,$value,$size,$y,$color,$bold=$false){
 $style=if($bold){[System.Drawing.FontStyle]::Bold}else{[System.Drawing.FontStyle]::Regular}
 $font=[System.Drawing.Font]::new('Segoe UI',$size,$style,[System.Drawing.GraphicsUnit]::Pixel)
 $brush=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($color))
 $format=[System.Drawing.StringFormat]::new();$format.Alignment=[System.Drawing.StringAlignment]::Center
 $rect=[System.Drawing.RectangleF]::new(80,$y,1760,150)
 $g.DrawString($value,$font,$brush,$rect,$format);$font.Dispose();$brush.Dispose();$format.Dispose()
}
foreach($kind in @('intro','outro')){
 $bitmap=[System.Drawing.Bitmap]::new(1920,1080,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($bitmap);$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias;$g.TextRenderingHint=[System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
 $g.Clear([System.Drawing.ColorTranslator]::FromHtml('#060B13'))
 # Fine technical lines and a restrained yellow luminous halo.
 $pen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(18,95,130,170),1)
 for($x=0;$x -le 1920;$x+=120){$g.DrawLine($pen,$x,0,$x,1080)}
 for($y=0;$y -le 1080;$y+=120){$g.DrawLine($pen,0,$y,1920,$y)}
 $pen.Dispose()
 $cy=if($kind -eq 'intro'){340}else{220}
 for($radius=330;$radius -gt 70;$radius-=5){$alpha=[int](1+5*(1-$radius/330));$halo=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb($alpha,255,202,32));$g.FillEllipse($halo,960-$radius,$cy-$radius,$radius*2,$radius*2);$halo.Dispose()}
 $logoSize=if($kind -eq 'intro'){300}else{230};$g.DrawImage($logo,[int](960-$logoSize/2),[int]($cy-$logoSize/2),$logoSize,$logoSize)
 if($kind -eq 'intro'){
  Text $g 'GRIDSHIFT' 100 530 '#F3F7FA' $true
  Text $g 'SIMULAÇÃO E GERENCIAMENTO' 28 675 '#FFCC25' $true
  Text $g 'DE LOGÍSTICA URBANA' 28 719 '#FFCC25' $true
  Text $g 'PROJETO INTEGRADOR  /  SENAI 2026' 22 920 '#8293A9'
 }else{
  Text $g 'GRIDSHIFT' 78 360 '#F3F7FA' $true
  Text $g 'A cidade não para. Sua empresa também não.' 35 480 '#FFCC25'
  Text $g 'Projeto Integrador — SENAI 2026' 26 566 '#DCE5EF'
  Text $g 'Anna Clara Lara Pereira — Multimídia' 23 696 '#C0CCDB'
  Text $g 'Geovanna Lopes Fagundes Dalla — Design Gráfico' 23 738 '#C0CCDB'
  Text $g 'Gustavo Henriques Barros — Programação de Jogos Digitais' 23 780 '#C0CCDB'
  Text $g 'Théo Henrique Barros dos Santos — Design Gráfico' 23 822 '#C0CCDB'
  Text $g 'Pedro do Nascimento Braga — Análise e Desenvolvimento de Sistemas' 23 864 '#C0CCDB'
 }
 $bitmap.Save((Join-Path $artPath "$kind.png"),[System.Drawing.Imaging.ImageFormat]::Png);$g.Dispose();$bitmap.Dispose()
}
$logo.Dispose()
Write-Output 'Motion graphic plates created from the official GS logo.'
