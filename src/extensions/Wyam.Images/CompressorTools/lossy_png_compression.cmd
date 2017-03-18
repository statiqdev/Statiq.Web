pngquant --speed 2 --skip-if-larger %1 --output %2
if not exist %2 copy %1 %2 /y

truepng /o4 %2
pngout %2 %2 /s1 /y /kpHYs
zopflipng --ohh %2 %2.png

if exist %2.png (
    copy %2.png %2 /y
    del %2.png
)