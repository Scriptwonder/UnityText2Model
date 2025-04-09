import os
import time
import torch
import gc
from PIL import Image
from hy3dgen.rembg import BackgroundRemover
from hy3dgen.shapegen import Hunyuan3DDiTFlowMatchingPipeline
from hy3dgen.texgen import Hunyuan3DPaintPipeline
import sys

torch.cuda.empty_cache()
gc.collect()

device = 'cuda'
pipeline = Hunyuan3DDiTFlowMatchingPipeline.from_pretrained(
    'tencent/Hunyuan3D-2mini',
    subfolder='hunyuan3d-dit-v2-mini-turbo',
    use_safetensors=False,
    device=device,
    cache_dir='cache/hunyuan3d-dit-v2-mini-turbo',
)
pipeline.enable_flashvdm(topk_mode='merge')
# pipeline.compile()

#texture generation
#takes extremely long at current step, will wait for further updates
#pipeline_texgen = Hunyuan3DPaintPipeline.from_pretrained('tencent/Hunyuan3D-2')



def run():
    # Clear memory before running inference
    torch.cuda.empty_cache()
    gc.collect()
    
    return pipeline(
        image=image,
        num_inference_steps=5,
        octree_resolution=380,
        num_chunks=20000,
        generator=torch.manual_seed(12345),
        output_type='trimesh'
    )[0]


save_dir = 'tmp/results/'
os.makedirs(save_dir, exist_ok=True)
unity_dir = ''
os.makedirs(unity_dir, exist_ok=True)
image_path='assets/example_images/004.png'

def runModelGen(objName='default', save_dir='tmp/results/'):
    # Ensure the save directory exists and normalize the path
    save_dir = os.path.normpath(save_dir)
    os.makedirs(save_dir, exist_ok=True)
    
    # Normalize object name (remove spaces and special characters)
    objName = objName.strip().replace(' ', '_')
    
    # Create the complete file path
    file_path = os.path.join(save_dir, f"{objName}.obj")
    
    # Ensure the directory for the file exists
    os.makedirs(os.path.dirname(file_path), exist_ok=True)
    
    for it in range(1):
        start_time = time.time()
        mesh = run()
        print("--- %s seconds ---" % (time.time() - start_time))
        mesh.export(file_path)
    
    #mesh = pipeline_texgen(mesh, image=image)
    #print("--- %s seconds ---" % (time.time() - start_time))
    #mesh.export(f'{save_dir}/run_{it+5}_textured.obj')
    
def main(image_path='Assets/Temp/generate_0.png', obj_name='default', save_dir='tmp/results/'):
    global image
    image = Image.open(image_path).convert("RGBA")
    if image.mode == 'RGB':
        rembg = BackgroundRemover()
        image = rembg(image)
    runModelGen(obj_name, save_dir)

if __name__ == '__main__':
    if len(sys.argv) > 3:
        print(sys.argv[1])
        main(sys.argv[1], sys.argv[2], sys.argv[3])
    else:
        main(image_path)
