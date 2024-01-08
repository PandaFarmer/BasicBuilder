import bpy, os, math, shutil, glob, bmesh, mathutils, sys, time
from functools import reduce
from itertools import product
from typing import Callable, Tuple

#just run by loading the script and pressing the play triangle in blender

pi = math.pi

def setupCamera(scene, c):

    scene.camera.rotation_euler[0] = c[0] * (pi / 180.0)
    scene.camera.rotation_euler[1] = c[1] * (pi / 180.0)
    scene.camera.rotation_euler[2] = c[2] * (pi / 180.0)

    scene.camera.location.x = c[3]
    scene.camera.location.y = c[4]
    scene.camera.location.z = c[5]

    return

#rotates about origin, coordinates only
def rotateCamera(scene, degreesRotation):
    x = scene.camera.location.x
    y = scene.camera.location.y
    z = scene.camera.location.z
    r = math.sqrt(x*x + y*y)

    radiansRotation = degreesRotation*(pi / 180.0)
    scene.camera.rotation_euler[2] = radiansRotation
    scene.camera.location.x = r*math.sin(radiansRotation) 
    scene.camera.location.y = -r*math.cos(radiansRotation)
    
def custom_reduce(f, identity, locations, index:int):
    result = identity
    for location in locations:
        result = f(result, location)
    return result[index]

zeroes_tuple = (0, 0, 0)
ones_tuple = (1, 1, 1)
max_size_tuple = (sys.maxsize, sys.maxsize, sys.maxsize)
min_size_tuple = (-sys.maxsize, -sys.maxsize, -sys.maxsize)

#runtime not compiletime error xd?
def sum_func(loc1:Tuple[float], loc2:Tuple[float]) -> Tuple[float]:
    return (loc1[0] + loc2[0], loc1[1] + loc2[1], loc1[2] + loc2[2])

def max_func(loc1:Tuple[float], loc2:Tuple[float]) -> Tuple[float]:
    return (max(loc1[0], loc2[0]), max(loc1[1], loc2[1]), max(loc1[2], loc2[2]))
    
def min_func(loc1:Tuple[float], loc2:Tuple[float]) -> Tuple[float]:
    return (min(loc1[0], loc2[0]), min(loc1[1], loc2[1]), min(loc1[2], loc2[2]))

#bad dont use
def normalize_to_origin():
    vertex_locations = []
    for obj in bpy.data.objects:
        if obj.type in ["LIGHT"]:
            obj.select_set(True)
            continue
        obj.select_set(False)
        if obj.type in ["CAMERA"]:
            continue
        coords = [(obj.matrix_world @ v.co) for v in obj.data.vertices]
        vertex_locations.extend(coords)
    bpy.ops.object.delete()
    #(loc1_x, loc1_y, loc1_z), (loc2_x, loc2_y, loc2_z)
    num_locations = len(vertex_locations)

    for obj in bpy.data.objects:
        if obj.type in ["CAMERA"]:
            continue
        # locations.append(obj.location)
        coords = [(obj.matrix_world @ v.co) for v in obj.data.vertices]
        vertex_locations.extend(coords)
    
    xsum = custom_reduce(sum_func, zeroes_tuple, locations, 0)
    ysum = custom_reduce(sum_func, zeroes_tuple, locations, 1)
    zsum = custom_reduce(sum_func, zeroes_tuple, locations, 2)
    avg_loc = (xsum/num_locations, ysum/num_locations, zsum/num_locations)
    
    for obj in bpy.data.objects:
        obj.select_set(True)
        
        obj.location.x = float(obj.location.x) - avg_loc[0]
        obj.location.y = float(obj.location.y) - avg_loc[1]
        obj.location.z = float(obj.location.z) - avg_loc[2]
        obj.select_set(False)
    return avg_loc
        
BASE_PATH = r"C:\Users\NoSpacesForWSL\Documents\blender29\RawAssets\FinalBlend"

def create_folder(folder_name):
    newpath = os.path.join(BASE_PATH,folder_name)
    if not os.path.exists(newpath):
        os.makedirs(newpath)
    return newpath
        
def render_rotations_and_save(save_file_prefix: str, render_rotation_degrees: int):
    for rotation_degree in range(0, 360, render_rotation_degrees): 
        rotateCamera(bpy.context.scene, rotation_degree)
        # Assume the last argument is image path
        imagePath = sys.argv[-1]
        if os.path.exists(imagePath):
            # Get file name:
            filename = bpy.path.basename(bpy.context.blend_data.filepath)

            # Remove .blend extension:
            filename = os.path.splitext(filename)[0]
            folder_path = create_folder(filename)
            imageBaseName = filename+save_file_prefix+str(rotation_degree)
            # bpy.context.scene.render.filepath += '-' + imageBaseName
            bpy.context.scene.render.filepath = os.path.join(folder_path, imageBaseName)
            # Render still image, automatically write to output path
            bpy.ops.render.render(write_still=True)
        else:
            print("Missing Image:", imagePath)
    return bpy.context.scene.render.filepath
    
            
#make sure to call setup_camera b4 this since location.z is being inced
def spawn_lights_and_render(render_rotation_degrees, 
                            isometric = False, 
                            profiles = False, 
                            top_down = False, 
                            icon = True,
                            light_distance_factor = 3, 
                            camera_distance_factor = 1.8,
                            corner_lights = True):
    
    vertex_locations = []
    for obj in bpy.data.objects:
        if obj.type in ["LIGHT"]:
            obj.select_set(True)
            continue
        obj.select_set(False)
        if obj.type in ["CAMERA"]:
            continue
        # locations.append(obj.location)
        try:
            coords = [(obj.matrix_world @ v.co) for v in obj.data.vertices]
        except Exception as e:
            print(e)
            continue
        vertex_locations.extend(coords)
    bpy.ops.object.delete()
    
    max_x = custom_reduce(max_func,min_size_tuple,vertex_locations,0)
    min_x = custom_reduce(min_func,max_size_tuple,vertex_locations,0)
    max_y = custom_reduce(max_func,min_size_tuple,vertex_locations,1)
    min_y = custom_reduce(min_func,max_size_tuple,vertex_locations,1)
    max_z = custom_reduce(max_func,min_size_tuple,vertex_locations,2)
    min_z = custom_reduce(min_func,max_size_tuple,vertex_locations,2)
    
    offset = ((max_x + min_x)/2, (max_y + min_y)/2, (max_z + min_z)/2)
    
    # for obj in bpy.data.objects:
    #     if obj.type in ["CAMERA"]:
    #         continue
    #     obj.select_set(True)
        
    #     obj.location.x = float(obj.location.x) - offset[0]
    #     obj.location.y = float(obj.location.y) - offset[1]
    #     obj.location.z = float(obj.location.z) - offset[2]
    #     obj.select_set(False)
        
    if not bpy.context.scene.camera:
        bpy.ops.object.camera_add(enter_editmode=False, align='VIEW', location=(0, 0, 0), rotation=(1.10871, 0.0132652, 1.14827), scale=(1, 1, 1))
    camera = bpy.context.scene.camera
    camera.location.x = float(camera.location.x) - offset[0]
    camera.location.y = float(camera.location.y) - offset[1]
    camera.location.z = float(camera.location.z) - offset[2]
    
    
    # bpy.context.scene.camera.location.z = bpy.context.scene.camera.location.z + (max_z+min_z)//2
    
    def scaled_mm():
        light_max_x = light_distance_factor*max_x
        light_min_x = light_distance_factor*min_x
        light_max_y = light_distance_factor*max_y
        light_min_y = light_distance_factor*min_y
        light_max_z = light_distance_factor*max_z
        light_min_z = light_distance_factor*min_z
        
        lights_pos = [light_max_x, light_min_x, light_max_y, light_min_y, light_max_z, light_min_z]
        quarter_avg_dist = .25*sum([abs(light_pos) for light_pos in lights_pos])/len(lights_pos)
    
        light_max_x = max([light_max_x, quarter_avg_dist])
        light_min_x = min([light_min_x, -quarter_avg_dist])
        light_max_y = max([light_max_y, quarter_avg_dist])
        light_min_y = min([light_min_y, -quarter_avg_dist])
        light_max_z = max([light_max_z, quarter_avg_dist])
        light_min_z = min([light_min_z, -quarter_avg_dist])
        
        return light_max_x, light_min_x, light_max_y, light_min_y, light_max_z, light_min_z
        
    #this is for 8 point lights on corners using itertools product
    def spawn_lights_corners():
        light_max_x, light_min_x, light_max_y, light_min_y, light_max_z, light_min_z = scaled_mm()
        mm_x = (light_max_x, light_min_x)
        mm_y = (light_max_y, light_min_y)
        mm_z = (light_max_z, light_min_z)
        max_coords = (max_x, max_y, max_z)
        min_coords = (min_x, min_y, min_z)
        
        light_data = bpy.data.lights.new(name="my-light-data", type='POINT')
        scaled_light_strength = 100*light_distance_factor*sum([max_x, min_x, max_y, min_y, max_z, min_z])/6
        light_data.energy = max([scaled_light_strength, 10])
        
        light_coords = list(product(mm_x, mm_y, mm_z))
        assert(len(light_coords) == 8)
        
        for num, light_coord in enumerate(light_coords):
            light_object = bpy.data.objects.new(name=f"my-light-corner-{num}", object_data=light_data)
            light_object.location = light_coord
            bpy.context.collection.objects.link(light_object)
    
    def spawn_lights():
        # Create light datablock
        light_data = bpy.data.lights.new(name="my-light-data", type='POINT')
        scaled_light_strength = 100*light_distance_factor*sum([max_x, min_x, max_y, min_y, max_z, min_z])/6
        light_data.energy = max([scaled_light_strength, 10])
        
        # Create new object, pass the light data 
        light_object_max_x = bpy.data.objects.new(name="my-light-max-x", object_data=light_data)
        light_object_min_x = bpy.data.objects.new(name="my-light-min-x", object_data=light_data)
        light_object_max_y = bpy.data.objects.new(name="my-light-max-y", object_data=light_data)
        light_object_min_y = bpy.data.objects.new(name="my-light-min-y", object_data=light_data)
        light_object_max_z = bpy.data.objects.new(name="my-light-max-z", object_data=light_data)
        light_object_min_z = bpy.data.objects.new(name="my-light-min-z", object_data=light_data)
        
        light_max_x, light_min_x, light_max_y, light_min_y, light_max_z, light_min_z = scaled_mm()
        
        light_object_max_x.location = (light_max_x, 0, 0)
        light_object_min_x.location = (light_min_x, 0, 0)
        light_object_max_y.location = (0, light_max_y, 0)
        light_object_min_y.location = (0, light_min_y, 0)
        light_object_max_z.location = (0, 0, light_max_z)
        light_object_min_z.location = (0, 0, light_min_z)
        
        # Link object to collection in context
        bpy.context.collection.objects.link(light_object_max_x)
        bpy.context.collection.objects.link(light_object_min_x)
        bpy.context.collection.objects.link(light_object_max_y)
        bpy.context.collection.objects.link(light_object_min_y)
        bpy.context.collection.objects.link(light_object_max_z)
        bpy.context.collection.objects.link(light_object_min_z)
    
    if corner_lights:
        spawn_lights_corners()
    else:
        spawn_lights()
    
    bpy.context.scene.render.film_transparent = True
    
    dimensions = [(max_x-min_x)/2, (max_y-min_y)/2, (max_z-min_z)/2]
    max_dim = max(dimensions)
    horizontal_max = max(dimensions[0:2])
    
    if isometric:
        setupCamera(bpy.context.scene, (45, 0, 90, camera_distance_factor*horizontal_max, 0, camera_distance_factor*horizontal_max))
        render_rotations_and_save("isometric_", render_rotation_degrees)
    
    if profiles:
        setupCamera(bpy.context.scene, (90, 0, 90, camera_distance_factor*horizontal_max, 0, 0))
        render_rotations_and_save("profile_", render_rotation_degrees)
    
    if top_down:
        setupCamera(bpy.context.scene, (0, 0, 90, 0, 0, camera_distance_factor*horizontal_max))
        render_rotations_and_save("top_down_", render_rotation_degrees)
        
    if icon:
        setupCamera(bpy.context.scene, (45, 0, 90, camera_distance_factor*horizontal_max, 0, camera_distance_factor*horizontal_max))
        image_path = render_rotations_and_save("icon_", 360)
        # filename = os.path.splitext(image_path)[0]
        print(f"image_path:\n{image_path}")
        


spawn_lights_and_render(45, isometric=False, 
                        profiles=False,
                        top_down=True, 
                        icon = True,
                        light_distance_factor = 5, camera_distance_factor = 7, corner_lights=True)



