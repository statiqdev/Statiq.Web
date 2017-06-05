pngout %1 %2 /s2 /y /kpHYs
if not exist %2 copy %1 %2 /y

zopflipng --ohh %2 %2.png

if exist %2.png (
    copy %2.png %2 /y
    del %2.png
)