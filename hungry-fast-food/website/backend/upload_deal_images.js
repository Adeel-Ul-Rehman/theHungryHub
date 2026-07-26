import cloudinary from 'cloudinary';
import dotenv from 'dotenv';
import fs from 'fs';
import path from 'path';

dotenv.config({ path: 'E:\\hungryHub\\hungry-fast-food\\website\\backend\\.env' });

const cloudinaryV2 = cloudinary.v2;
cloudinaryV2.config({
    cloud_name: process.env.CLOUDINARY_CLOUD_NAME,
    api_key: process.env.CLOUDINARY_API_KEY,
    api_secret: process.env.CLOUDINARY_API_SECRET
});

const imageDir = 'E:\\hungryHub\\tools\\images';
const mappingPath = 'E:\\hungryHub\\tools\\images\\image_mapping.json';

async function run() {
    let mapping = {};
    if (fs.existsSync(mappingPath)) {
        mapping = JSON.parse(fs.readFileSync(mappingPath, 'utf8'));
    }

    console.log("Starting upload of deal images to Cloudinary...");
    for (let i = 1; i <= 15; i++) {
        const filename = `deal_${i}.png`;
        const filepath = path.join(imageDir, filename);
        
        if (!fs.existsSync(filepath)) {
            console.log(`Warning: File ${filename} does not exist yet.`);
            continue;
        }

        console.log(`Uploading ${filename}...`);
        try {
            const res = await cloudinaryV2.uploader.upload(filepath, {
                folder: 'hungryhub/deals',
                use_filename: true,
                unique_filename: false
            });
            mapping[filename] = res.secure_url;
            console.log(`  Uploaded: ${filename} -> ${res.secure_url}`);
        } catch (err) {
            console.error(`  Failed to upload ${filename}:`, err);
        }
    }

    fs.writeFileSync(mappingPath, JSON.stringify(mapping, null, 2), 'utf8');
    console.log("Success: image_mapping.json updated successfully!");
}

run();
