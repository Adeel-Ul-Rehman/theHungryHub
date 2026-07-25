"""
Fast Food Product Image Generator using Pollinations.ai (Free API)

This script automatically generates 55 high-quality, studio-ready, white-background
product images for the Hungry Hub website. It uses the Pollinations.ai API to
generate images using the Flux model for free, without requiring any API keys.

Requirements:
    pip install requests pillow

Usage:
    python tools/generate_images.py
"""

import os
import sys
import time
import urllib.parse
import requests
from PIL import Image
from io import BytesIO

# Product name to prompt mapping for all 55 items
PRODUCT_PROMPTS = {
    # Burgers (7)
    "zinger_burger": (
        "A premium studio food photograph of a Zinger Burger, isolated on a solid pure white background. "
        "Features a thick, extra-crispy golden-brown fried chicken breast fillet, fresh green lettuce leaves, "
        "and a swirl of creamy white mayonnaise inside a soft toasted sesame seed bun. Centered, professional "
        "studio lighting, high resolution, no text, no watermark, 8k."
    ),
    "zinger_tower_burger": (
        "A premium studio food photograph of a Zinger Tower Burger, isolated on a solid pure white background. "
        "Features a thick, extra-crispy golden fried chicken fillet, a golden crispy hashbrown, a slice of melted "
        "yellow cheddar cheese, fresh lettuce, and mayonnaise in a soft toasted sesame bun. High stack, centered, "
        "studio lighting, no text, no watermark."
    ),
    "chicken_burger": (
        "A premium studio food photograph of a classic chicken burger, isolated on a solid pure white background. "
        "Features a grilled succulent chicken patty, fresh green lettuce, a tomato slice, and classic mayonnaise "
        "inside a soft glossy burger bun. Centered, professional studio lighting, no text, no watermark."
    ),
    "chicken_cheese_burger": (
        "A premium studio food photograph of a chicken cheese burger, isolated on a solid pure white background. "
        "Features a hot grilled chicken patty topped with a slice of melted yellow cheddar cheese, lettuce, tomato, "
        "and mayo in a soft bun. Centered, delicious melted cheese detail, studio lighting, no text, no watermark."
    ),
    "zinger_cheese_burger": (
        "A premium studio food photograph of a Zinger Cheese Burger, isolated on a solid pure white background. "
        "Features a crispy golden fried chicken fillet topped with a slice of melted yellow cheddar cheese, lettuce, "
        "and mayonnaise inside a toasted sesame bun. Centered, professional food styling, studio lighting, no text."
    ),
    "chapli_cheese_burger": (
        "A premium studio food photograph of a Chapli Cheese Burger, isolated on a solid pure white background. "
        "Features a traditional spiced beef chapli kebab patty, a slice of melted cheese, red onion rings, tomato "
        "slice, and a drizzle of mint chutney sauce in a soft bun. Centered, fusion style, studio lighting, no text."
    ),
    "double_decker_burger": (
        "A premium studio food photograph of a Double Decker Burger, isolated on a solid pure white background. "
        "Features two layers of extra-crispy golden fried chicken fillets, two slices of melted yellow cheddar cheese, "
        "fresh lettuce, and creamy mayonnaise, stacked high in a double decker sesame bun. High stack, centered, "
        "studio lighting, no text."
    ),

    # Appetizers (4)
    "french_fries": (
        "A premium studio food photograph of crispy golden French fries, isolated on a solid pure white background. "
        "The fries are perfectly seasoned, crispy on the outside, and served in a classic red cardboard fry container. "
        "Centered, studio lighting, professional food styling, no text."
    ),
    "mayo_fries": (
        "A premium studio food photograph of crispy golden French fries, isolated on a solid pure white background. "
        "The fries are piled high and topped with a generous, artistic drizzle of creamy white mayonnaise. Centered, "
        "studio lighting, no text."
    ),
    "loaded_fries": (
        "A premium studio food photograph of loaded French fries, isolated on a solid pure white background. Crispy "
        "golden fries served in a white bowl, loaded with melted cheddar cheese sauce, sliced pickled jalapeños, and "
        "bits of seasoned grilled chicken. Centered, gourmet look, studio lighting, no text."
    ),
    "pizza_fries": (
        "A premium studio food photograph of baked Pizza Fries, isolated on a solid pure white background. French fries "
        "topped with zesty pizza sauce, a thick layer of melted mozzarella cheese, sliced black olives, and diced "
        "chicken tikka chunks. Golden-brown cheese strings, centered, studio lighting, no text."
    ),

    # Sandwiches (4)
    "chicken_classic_sandwich": (
        "A premium studio food photograph of a classic chicken sandwich, isolated on a solid pure white background. "
        "Features two slices of toasted white bread filled with a creamy mixture of shredded chicken, mayonnaise, "
        "and finely chopped celery and lettuce. Cut diagonally, centered, studio lighting, no text."
    ),
    "hungry_special_sandwich": (
        "A premium studio food photograph of a multi-layered signature club sandwich, isolated on a solid pure "
        "white background. Features layers of toasted bread, grilled chicken tikka chunks, a fried egg, melted "
        "cheese, lettuce, tomato, and a secret house sauce. Cut into triangles, stacked, centered, studio lighting, no text."
    ),
    "malai_botti_sandwich": (
        "A premium studio food photograph of a Malai Boti Sandwich, isolated on a solid pure white background. "
        "Features toasted bread slices stuffed with creamy, flame-grilled chicken malai boti chunks, fresh onion rings, "
        "and a drizzle of garlic mayo chutney. Centered, studio lighting, no text."
    ),
    "club_sandwich": (
        "A premium studio food photograph of a classic double-decker Club Sandwich, isolated on a solid pure white "
        "background. Features layers of toasted white bread, smoked turkey or chicken slices, a fried egg, crispy beef "
        "bacon, cheese, lettuce, tomato, and mayonnaise. Cut into triangles, secured with toothpicks, centered, studio "
        "lighting, no text."
    ),

    # Shawarma (5)
    "chicken_shawarma": (
        "A premium studio food photograph of a Chicken Shawarma wrap, isolated on a solid pure white background. "
        "Features a warm toasted pita bread wrap stuffed with tender shaved roasted chicken, pickled cucumbers, and "
        "rich garlic sauce (toum). Slightly open to show the filling, centered, studio lighting, no text."
    ),
    "zinger_shawarma": (
        "A premium studio food photograph of a Zinger Shawarma wrap, isolated on a solid pure white background. "
        "Features a warm pita wrap containing crispy golden-brown fried chicken zinger strips, lettuce, and a drizzle "
        "of spicy mayo. Centered, studio lighting, no text."
    ),
    "chicken_malai_botti_shawarma": (
        "A premium studio food photograph of a Chicken Malai Boti Shawarma wrap, isolated on a solid pure white "
        "background. Features pita bread wrapped around creamy flame-grilled malai boti chicken chunks, sliced onions, "
        "and a drizzle of mint garlic sauce. Centered, studio lighting, no text."
    ),
    "chicken_cheese_shawarma": (
        "A premium studio food photograph of a Chicken Cheese Shawarma wrap, isolated on a solid pure white background. "
        "Features pita bread filled with roasted chicken chunks, melted mozzarella cheese, and rich garlic sauce, "
        "wrapped neatly. Centered, studio lighting, no text."
    ),
    "zinger_cheese_shawarma": (
        "A premium studio food photograph of a Zinger Cheese Shawarma wrap, isolated on a solid pure white background. "
        "Features a warm pita wrap containing crispy fried chicken zinger strips, melted cheddar cheese, shredded "
        "lettuce, and mayonnaise. Centered, studio lighting, no text."
    ),

    # Pizza Regular Flavors (8)
    "chicken_tikka_pizza": (
        "A premium studio food photograph of a whole Chicken Tikka Pizza, isolated on a solid pure white background. "
        "Features a golden-brown hand-stretched crust topped with spicy red chicken tikka chunks, sliced red onions, "
        "bell peppers, melted mozzarella cheese, and fresh coriander leaves. Angle shot, professional styling, "
        "studio lighting, no text."
    ),
    "chicken_fajita_pizza": (
        "A premium studio food photograph of a whole Chicken Fajita Pizza, isolated on a solid pure white background. "
        "Features a hand-stretched pizza crust topped with seasoned fajita chicken chunks, colorful sliced bell peppers, "
        "onions, and melted mozzarella cheese. Centered, studio lighting, no text."
    ),
    "hot_and_spicy_pizza": (
        "A premium studio food photograph of a whole Hot & Spicy Pizza, isolated on a solid pure white background. "
        "Features a pizza topped with spicy shredded chicken, sliced jalapeños, chili flakes, red onions, and melted "
        "mozzarella cheese. Flame-grilled crust, centered, studio lighting, no text."
    ),
    "achar_ghost_pizza": (
        "A premium studio food photograph of a whole Achari Chicken Pizza, isolated on a solid pure white background. "
        "Features a pizza topped with achari-style pickled spiced chicken chunks, green chilies, red onions, and melted "
        "mozzarella cheese. Centered, studio lighting, no text."
    ),
    "veggie_lover_pizza": (
        "A premium studio food photograph of a whole Veggie Lover Pizza, isolated on a solid pure white background. "
        "Features a colorful pizza topped with sliced bell peppers, red onions, sweet corn, mushrooms, black olives, "
        "cherry tomatoes, and melted mozzarella cheese. Centered, studio lighting, no text."
    ),
    "supreme_special_pizza": (
        "A premium studio food photograph of a whole Supreme Pizza, isolated on a solid pure white background. "
        "Features a loaded pizza topped with chicken chunks, beef pepperoni slices, mushrooms, black olives, green "
        "bell peppers, red onions, and a thick layer of melted mozzarella. Centered, studio lighting, no text."
    ),
    "peri_peri_pizza": (
        "A premium studio food photograph of a whole Peri Peri Pizza, isolated on a solid pure white background. "
        "Features a pizza topped with peri-peri seasoned chicken chunks, red onions, and bell peppers, finished "
        "with a beautiful zigzag drizzle of spicy orange peri peri sauce. Centered, studio lighting, no text."
    ),
    "chicken_tandoori_pizza": (
        "A premium studio food photograph of a whole Chicken Tandoori Pizza, isolated on a solid pure white background. "
        "Features a hand-tossed pizza crust topped with red tandoori chicken chunks, sliced red onions, green chilies, "
        "and melted mozzarella cheese, garnished with fresh cilantro. Centered, studio lighting, no text."
    ),

    # Pizza Special Flavors (8)
    "hungry_special_pizza": (
        "A premium studio food photograph of a signature whole Deep Dish Pizza, isolated on a solid pure white "
        "background. Features a thick golden crust loaded with special spiced chicken, chicken sausages, black olives, "
        "sliced mushrooms, and a secret double-cheese blend. Centered, premium food styling, studio lighting, no text."
    ),
    "crown_crust_pizza": (
        "A premium studio food photograph of a whole Crown Crust Pizza, isolated on a solid pure white background. "
        "Features a unique crown crust where the edge of the pizza crust consists of small dough balls filled with "
        "cream cheese. Topped with grilled chicken, onions, and bell peppers. Centered, studio lighting, no text."
    ),
    "kabab_crust_pizza": (
        "A premium studio food photograph of a whole Kabab Crust Pizza, isolated on a solid pure white background. "
        "Features a pizza crust embedded with seekh kababs along the outer rim. Topped with spicy chicken tikka chunks, "
        "red onions, and melted mozzarella cheese. Centered, studio lighting, no text."
    ),
    "behari_kbab_pizza": (
        "A premium studio food photograph of a whole Bihari Kabab Pizza, isolated on a solid pure white background. "
        "Features a pizza topped with smoky, tender Bihari-style chicken kebab chunks, sliced red onions, a light "
        "drizzle of green mint chutney, and melted mozzarella cheese. Centered, studio lighting, no text."
    ),
    "lasanza_pizza": (
        "A premium studio food photograph of a whole Lasagna Pizza, isolated on a solid pure white background. "
        "Features a pizza with layers of rich meat sauce, cheese, lasagna pasta sheets embedded, and heavily melted "
        "mozzarella cheese on top, golden-brown baked. Centered, studio lighting, no text."
    ),
    "mughalai_pizza": (
        "A premium studio food photograph of a whole Mughlai Pizza, isolated on a solid pure white background. "
        "Features a gourmet pizza topped with rich, creamy Mughlai chicken chunks, flaked almonds, green cardamom notes, "
        "and a premium cheese blend. Centered, studio lighting, no text."
    ),
    "malai_botti_pizza": (
        "A premium studio food photograph of a whole Malai Boti Pizza, isolated on a solid pure white background. "
        "Features a pizza topped with creamy, flame-grilled malai boti chicken chunks, sliced green chilies, red onions, "
        "and melted mozzarella cheese. Centered, studio lighting, no text."
    ),
    "mushroom_pluse_pizza": (
        "A premium studio food photograph of a whole Mushroom Pizza, isolated on a solid pure white background. "
        "Features a pizza loaded with sliced white button mushrooms, chicken chunks, black olives, and extra melted "
        "mozzarella cheese. Centered, studio lighting, no text."
    ),

    # Rolls (7)
    "tortilla_wrap": (
        "A premium studio food photograph of a Tortilla Wrap, isolated on a solid pure white background. Features a "
        "grilled flour tortilla rolled and filled with grilled chicken strips, shredded lettuce, diced tomatoes, and "
        "garlic mayonnaise. Centered, cut in half to show the filling, studio lighting, no text."
    ),
    "turkish_wrap": (
        "A premium studio food photograph of a Turkish Wrap (Durum), isolated on a solid pure white background. "
        "Features a traditional flatbread wrap stuffed with spiced grilled chicken or meat, fresh parsley, red onions, "
        "tomatoes, and a light garlic sauce. Centered, studio lighting, no text."
    ),
    "kabab_roll": (
        "A premium studio food photograph of a Kabab Roll, isolated on a solid pure white background. Features a "
        "golden crispy paratha or flatbread wrap containing a flame-grilled seekh kabab, sliced onions, and a drizzle "
        "of green mint chutney. Centered, studio lighting, no text."
    ),
    "chicken_pratha_roll": (
        "A premium studio food photograph of a Chicken Paratha Roll, isolated on a solid pure white background. "
        "Features a crispy, flaky golden-fried paratha wrapped around spicy chicken tikka chunks, sliced onions, "
        "and tangy mint yogurt chutney. Centered, studio lighting, no text."
    ),
    "zinger_pratha_roll": (
        "A premium studio food photograph of a Zinger Paratha Roll, isolated on a solid pure white background. "
        "Features a flaky golden-brown paratha wrapped around crispy zinger chicken strips, lettuce, and creamy "
        "mayonnaise. Centered, studio lighting, no text."
    ),
    "cheese_chicken_pratha_roll": (
        "A premium studio food photograph of a Cheese Chicken Paratha Roll, isolated on a solid pure white background. "
        "Features a flaky golden-brown paratha wrap filled with chicken tikka chunks, melted cheddar cheese, onions, "
        "and green chutney. Centered, studio lighting, no text."
    ),
    "cheese_zinger_pratha_roll": (
        "A premium studio food photograph of a Cheese Zinger Paratha Roll, isolated on a solid pure white background. "
        "Features a flaky golden-brown paratha wrap filled with crispy chicken zinger strips, melted cheddar cheese, "
        "and creamy garlic mayo. Centered, studio lighting, no text."
    ),

    # Pastas (5)
    "hungry_special_pasta": (
        "A premium studio food photograph of a signature baked pasta dish, isolated on a solid pure white background. "
        "Features penne pasta tossed in a creamy rich white sauce with grilled chicken chunks and mushrooms, baked in a "
        "white ceramic dish topped with bubbly, golden-brown melted mozzarella cheese. Centered, studio lighting, no text."
    ),
    "crunchy_pasta": (
        "A premium studio food photograph of Crunchy Chicken Pasta, isolated on a solid pure white background. "
        "Features pasta tossed in a spicy red marinara sauce, topped with crispy golden-brown fried chicken bites, "
        "melted mozzarella cheese, and fresh basil. Centered, studio lighting, no text."
    ),
    "creamy_pasta": (
        "A premium studio food photograph of Creamy Pasta, isolated on a solid pure white background. Features penne "
        "pasta tossed in a rich, velvety parmesan white cream sauce with sautéed mushrooms, garlic, and fresh herbs, "
        "served in a white bowl. Centered, studio lighting, no text."
    ),
    "alfredo_pasta": (
        "A premium studio food photograph of classic Fettuccine Alfredo, isolated on a solid pure white background. "
        "Features long ribbon fettuccine pasta tossed in a rich, creamy butter and parmesan cheese alfredo sauce, "
        "garnished with fresh parsley. Centered, studio lighting, no text."
    ),
    "chicken_alfredo_pasta": (
        "A premium studio food photograph of Chicken Fettuccine Alfredo, isolated on a solid pure white background. "
        "Features fettuccine pasta in a rich white alfredo sauce, topped with beautifully grilled, sliced chicken breast "
        "fillets and fresh parsley. Centered, studio lighting, no text."
    ),

    # Wings & Nuggets (4)
    "hot_wings": (
        "A premium studio food photograph of Hot Wings, isolated on a solid pure white background. A plate of six "
        "crispy fried chicken wings coated in a glossy, vibrant red buffalo hot sauce. Served with a small bowl of "
        "ranch dip. Centered, studio lighting, no text."
    ),
    "oven_baked_wings": (
        "A premium studio food photograph of Oven Baked Wings, isolated on a solid pure white background. A plate of six "
        "golden-brown, glazed oven-baked chicken wings seasoned with herbs and garlic. Centered, studio lighting, no text."
    ),
    "nuggets": (
        "A premium studio food photograph of Chicken Nuggets, isolated on a solid pure white background. A pile of "
        "crispy, golden-brown fried chicken nuggets served on a white plate with a side of ketchup. Centered, "
        "studio lighting, no text."
    ),
    "garlic_wings": (
        "A premium studio food photograph of Garlic Wings, isolated on a solid pure white background. A plate of six "
        "crispy chicken wings tossed in rich garlic butter, sprinkled with grated parmesan and fresh chopped parsley. "
        "Centered, studio lighting, no text."
    ),

    # Add-ons (3)
    "dip_sauce": (
        "A premium studio food photograph of creamy dip sauce, isolated on a solid pure white background. Served in a "
        "small, clean white ceramic ramekin. Rich, smooth, white garlic-herb sauce. Centered, studio lighting, no text."
    ),
    "special_sauce": (
        "A premium studio food photograph of signature special sauce, isolated on a solid pure white background. "
        "Served in a small, clean white ceramic ramekin. Creamy, smooth orange-pink burger sauce. Centered, "
        "studio lighting, no text."
    ),
    "cheese_slice": (
        "A premium studio food photograph of a single perfect square slice of yellow cheddar cheese, isolated on a "
        "solid pure white background. Clean edges, smooth texture, studio lighting, centered, no text."
    )
}

def main():
    # Define output directory (saved relative to the script inside tools/images)
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_dir = os.path.join(script_dir, "images")
    os.makedirs(output_dir, exist_ok=True)
    print(f"[*] Target directory: {output_dir}")

    total_items = len(PRODUCT_PROMPTS)
    completed = 0
    skipped = 0
    failed = 0

    print(f"[*] Starting FREE image generation using Pollinations.ai (Flux) for {total_items} items.")
    print("-" * 80)

    for idx, (filename, prompt) in enumerate(PRODUCT_PROMPTS.items(), 1):
        target_path = os.path.join(output_dir, f"{filename}.png")
        display_name = filename.replace("_", " ").title()

        # Resume logic: check if the file already exists
        if os.path.exists(target_path):
            print(f"[{idx}/{total_items}] Skipped (exists): '{display_name}' -> {filename}.png")
            skipped += 1
            continue

        print(f"[{idx}/{total_items}] Generating: '{display_name}'...")
        
        # URL encode the prompt
        encoded_prompt = urllib.parse.quote(prompt)
        url = f"https://image.pollinations.ai/prompt/{encoded_prompt}?width=1024&height=1024&nologo=true"

        max_retries = 5
        retry_delay = 10  # Initial delay for retry in seconds
        success_flag = False

        for attempt in range(1, max_retries + 1):
            try:
                # Request the image (using 45s timeout to handle slow generations)
                response = requests.get(url, timeout=45)
                
                if response.status_code == 200:
                    # Save the image content
                    image = Image.open(BytesIO(response.content))
                    image.save(target_path, "PNG")
                    print(f"    Success: Saved to tools/images/{filename}.png")
                    success_flag = True
                    completed += 1
                    break
                elif response.status_code == 429:
                    print(f"    [!] Rate limited (429). Retrying in {retry_delay}s... (Attempt {attempt}/{max_retries})")
                    time.sleep(retry_delay)
                    retry_delay *= 2  # Exponential backoff
                else:
                    print(f"    [!] HTTP Error {response.status_code}. Retrying in {retry_delay}s... (Attempt {attempt}/{max_retries})")
                    time.sleep(retry_delay)
                    retry_delay *= 1.5

            except Exception as e:
                print(f"    [!] Connection/Read Error: {e}. Retrying in {retry_delay}s... (Attempt {attempt}/{max_retries})")
                time.sleep(retry_delay)
                retry_delay *= 1.5

        if not success_flag:
            print(f"    [!] Failed to generate '{display_name}' after {max_retries} attempts.")
            failed += 1

        # Base sleep time between successful items to avoid hitting rate limits
        if success_flag:
            time.sleep(8)


    print("-" * 80)
    print("Generation complete!")
    print(f"  Total items: {total_items}")
    print(f"  Generated:   {completed}")
    print(f"  Skipped:     {skipped}")
    print(f"  Failed:      {failed}")
    print("-" * 60)
    print(f"Images are saved in: {output_dir}")

if __name__ == "__main__":
    main()
