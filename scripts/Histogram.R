library(EBImage)

image = readImage("upload/file.png")
grayImage = channel(image,"gray")
grayScaled = floor(grayImage * 255)
histogram = graphics::hist(grayScaled, plot = FALSE)

histogram
