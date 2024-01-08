# packages_path = "C:\\Users\\NoSpacesForWSL\\AppData\\Roaming\\Python\\Python310\\Scripts" + "\\..\\site-packages"
# sys.path.insert(0, packages_path )
import sys, os, re
from PIL import Image

#single argshould be folder that the <AssetName>icon_0.png file is in

def resize_image_and_save(image, save_path_no_ext, size=(128, 128)):
    save_path_no_ext = save_path_no_ext.split("_0")[0]
    image.thumbnail(size, Image.Resampling.LANCZOS)
    image.save(f"{save_path_no_ext}_size{size[0]}x{size[1]}.png")

def crop_image(image):
    imageBox = image.getbbox()
    print(f"cropping image with bbox: {imageBox}")
    cropped = image.crop(imageBox)
    return cropped

def crop_all_images_in_folderpath():
    folder_path = sys.argv[-1]

    folder_path = re.sub(r"\\", r"/", folder_path.split("Users")[-1])

    folder_path = "/mnt/c/Users"+folder_path

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
        
def create_thumbnail_icons():
    image_path = sys.argv[-1]

    file_path_suffix = "/" + image_path.split("\\")[-1]+"icon_0.png"

    image_path = re.sub(r"\\", r"/", image_path.split("Users")[-1])
    image_path = "/mnt/c/Users"+image_path+file_path_suffix
    image = Image.open(image_path)
    image = crop_image(image)
    resize_image_and_save(image, image_path, (128, 128));
    resize_image_and_save(image, image_path, (64, 64));
    resize_image_and_save(image, image_path, (32, 32));

if __name__ == "__main__":
    crop_all_images_in_folderpath()
    create_thumbnail_icons()