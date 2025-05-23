import argparse
import os
import math
from PIL import Image, ImageDraw

def generate_circular_grid(
    output_prefix="circular_grid",
    width=1420,
    height=1420,
    rows=None,
    cols=None,
    circle_radius=16,
    spacing=60,
    bg_color=(255, 255, 255),
    circle_color=(0, 0, 0),
    padding=0
):
    """
    Generate a PNG image with a grid of circles contained within a circular boundary,
    and save it as 4 separate quadrant images.
    
    Args:
        output_prefix: Prefix for output filenames
        width: Width of the full output image
        height: Height of the full output image
        rows: Number of rows in the grid (calculated automatically if None)
        cols: Number of columns in the grid (calculated automatically if None)
        circle_radius: Radius of each circle in pixels
        spacing: Space between circles (if None, calculated based on circle_radius)
        bg_color: Background color as RGB tuple
        circle_color: Circle color as RGB tuple
        padding: Padding from the edge of the image
    """
    # Create a new image with specified background color
    img = Image.new('RGB', (width, height), bg_color)
    draw = ImageDraw.Draw(img)
    
    # Calculate the center and maximum radius of the circular boundary
    center_x = width // 2
    center_y = height // 2
    max_boundary_radius = min(width, height) // 2 - padding
    
    # Calculate spacing if not provided
    if spacing is None:
        spacing = circle_radius // 2  # Default spacing is half of the circle radius
    
    # Calculate grid dimensions if not provided
    if rows is None or cols is None:
        # Calculate based on circle size and spacing
        circle_diameter = 2 * circle_radius
        effective_spacing = spacing + circle_diameter
        
        if rows is None:
            rows = int((2 * max_boundary_radius) / effective_spacing)
        if cols is None:
            cols = int((2 * max_boundary_radius) / effective_spacing)
    
    # Calculate the total grid width and height
    total_grid_width = cols * (2 * circle_radius + spacing) - spacing
    total_grid_height = rows * (2 * circle_radius + spacing) - spacing
    
    # Calculate the starting position to center the grid
    start_x = center_x - total_grid_width // 2
    start_y = center_y - total_grid_height // 2
    
    # Draw circles in a grid pattern, but only if they're within the circular boundary
    for row in range(rows):
        for col in range(cols):
            # Calculate the center of this circle in the grid
            x = start_x + circle_radius + col * (2 * circle_radius + spacing)
            y = start_y + circle_radius + row * (2 * circle_radius + spacing)
            
            # Check if this circle is within the circular boundary
            distance_to_center = math.sqrt((x - center_x)**2 + (y - center_y)**2)
            if distance_to_center + circle_radius <= max_boundary_radius:
                # Draw the circle
                draw.ellipse(
                    [(x - circle_radius, y - circle_radius),
                     (x + circle_radius, y + circle_radius)],
                    fill=circle_color,
                    outline=circle_color
                )
    
    # Ensure the output directory exists
    os.makedirs(os.path.dirname(output_prefix) if os.path.dirname(output_prefix) else '.', exist_ok=True)
    
    # Split the image into 4 quadrants and save each one
    half_width = width // 2
    half_height = height // 2
    
    # Top-left quadrant
    top_left = img.crop((0, 0, half_width, half_height))
    top_left_path = f"{output_prefix}_top_left.png"
    top_left.save(top_left_path)
    
    # Top-right quadrant
    top_right = img.crop((half_width, 0, width, half_height))
    top_right_path = f"{output_prefix}_top_right.png"
    top_right.save(top_right_path)
    
    # Bottom-left quadrant
    bottom_left = img.crop((0, half_height, half_width, height))
    bottom_left_path = f"{output_prefix}_bottom_left.png"
    bottom_left.save(bottom_left_path)
    
    # Bottom-right quadrant
    bottom_right = img.crop((half_width, half_height, width, height))
    bottom_right_path = f"{output_prefix}_bottom_right.png"
    bottom_right.save(bottom_right_path)
    
    # Also save the full image for reference
    full_image_path = f"{output_prefix}_full.png"
    img.save(full_image_path)
    
    print(f"Circular grid generated and saved as quadrants:")
    print(f"- {top_left_path}")
    print(f"- {top_right_path}")
    print(f"- {bottom_left_path}")
    print(f"- {bottom_right_path}")
    print(f"Full image saved as {full_image_path}")
    
    return img

def main():
    parser = argparse.ArgumentParser(description='Generate a grid of circles contained within a circular boundary, split into 4 quadrants.')
    parser.add_argument('--output', type=str, default='circular_grid', help='Output file prefix')
    parser.add_argument('--width', type=int, default=800, help='Image width in pixels')
    parser.add_argument('--height', type=int, default=800, help='Image height in pixels')
    parser.add_argument('--rows', type=int, help='Number of rows in the grid (calculated automatically if not provided)')
    parser.add_argument('--cols', type=int, help='Number of columns in the grid (calculated automatically if not provided)')
    parser.add_argument('--radius', type=int, default=20, help='Radius of each circle in pixels')
    parser.add_argument('--spacing', type=int, help='Space between circles in pixels (edge to edge)')
    parser.add_argument('--bg-color', type=str, default='white', help='Background color (name or hex)')
    parser.add_argument('--circle-color', type=str, default='black', help='Circle color (name or hex)')
    parser.add_argument('--padding', type=int, default=20, help='Padding from the edge of the image')
    
    args = parser.parse_args()
    
    # Convert color strings to RGB tuples
    from PIL import ImageColor
    bg_color = ImageColor.getrgb(args.bg_color)
    circle_color = ImageColor.getrgb(args.circle_color)
    
    generate_circular_grid(
        output_prefix=args.output,
        width=1427,
        height=1427,
        rows=15,
        cols=15,
        circle_radius=18,
        spacing=54,
        bg_color=bg_color,
        circle_color=circle_color,
        padding=0
    )

if __name__ == "__main__":
    main()