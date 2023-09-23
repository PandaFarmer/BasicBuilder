# packages_path = "C:\\Users\\NoSpacesForWSL\\AppData\\Roaming\\Python\\Python310\\Scripts" + "\\..\\site-packages"
# sys.path.insert(0, packages_path )
import sys, os, re
from PIL import Image

def resize_image_and_save(image, save_path_no_ext, size=(128, 128)):
    save_path_no_ext = save_path_no_ext.split("_0")[0]
    image.thumbnail(size, Image.Resampling.LANCZOS)
    image.save(f"{save_path_no_ext}_size{size[0]}x{size[1]}.png")

def crop_image(image):
    imageBox = image.getbbox()
    cropped = image.crop(imageBox)
    return cropped

image_path = sys.argv[-1]
image_path = re.sub(r"\\", r"/", image_path.split("Users")[-1])
image_path = "/mnt/c/Users"+image_path
image = Image.open(image_path)
image = crop_image(image)
resize_image_and_save(image, image_path, (128, 128));
resize_image_and_save(image, image_path, (64, 64));
resize_image_and_save(image, image_path, (32, 32));

