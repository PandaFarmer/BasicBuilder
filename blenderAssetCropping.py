import sys, os, re
from PIL import Image

def crop_image(image):
    imageBox = image.getbbox()
    print(f"cropping image with bbox: {imageBox}")
    cropped = image.crop(imageBox)
    return cropped

folder_path = sys.argv[-1]
folder_path = re.sub(r"\\", r"/", folder_path.split("Users")[-1])
folder_path = "/mnt/c/Users"+folder_path

folder = folder_path.split("/")[-1]

for image_path in os.listdir(folder_path):
    image_path = os.path.join(folder_path, image_path)
    print(f"Processing image_path: {image_path}")
    # if os.path.splitext(image_path)[-1] != "png":
    #     continue
    # if "icon" in image_path:
    #     continue
    
    image = Image.open(image_path)
    print(f"image size before crop: {image.size}")
    image = crop_image(image)
    print(f"image size after crop: {image.size}")
    image.save(image_path)