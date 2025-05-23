import os
import requests
import argparse
from tqdm import tqdm

def download_file(url, destination, force_refresh=False):
    """
    Download a file from a URL and save it to the specified destination.
    
    Args:
        url (str): The URL to download from
        destination (str): The local path to save the file to
        force_refresh (bool): Whether to download the file even if it already exists
    
    Returns:
        bool: True if download was successful, False otherwise
    """
    # Check if file already exists
    if os.path.exists(destination) and not force_refresh:
        print(f"File {destination} already exists, skipping.")
        return True
    
    # Create directory if it doesn't exist
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    
    try:
        # Send a GET request to the URL
        response = requests.get(url, stream=True)
        response.raise_for_status()  # Raise an exception for HTTP errors
        
        # Get file size if available
        total_size = int(response.headers.get('content-length', 0))
        
        # Initialize progress bar
        progress_bar = tqdm(total=total_size, unit='B', unit_scale=True, 
                           desc=os.path.basename(destination))
        
        # Write the file
        with open(destination, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)
                    progress_bar.update(len(chunk))
        
        progress_bar.close()
        print(f"Successfully downloaded {url} to {destination}")
        return True
    
    except requests.exceptions.HTTPError as e:
        print(f"HTTP Error: {e}")
    except requests.exceptions.ConnectionError as e:
        print(f"Connection Error: {e}")
    except requests.exceptions.Timeout as e:
        print(f"Timeout Error: {e}")
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")
    except IOError as e:
        print(f"I/O Error: {e}")
    
    print(f"Failed to download {url}")
    return False

def main():
    # Parse command line arguments
    parser = argparse.ArgumentParser(description='Download astronomical ephemeris files from JPL')
    parser.add_argument('--refresh', action='store_true', 
                        help='Force redownload of files even if they already exist')
    args = parser.parse_args()
    
    refresh = args.refresh
    
    # List of files to download
    required_files = []
    for year in range(1550, 2551, 100):
        required_files.append({
            'url': 'https://ssd.jpl.nasa.gov/ftp/eph/planets/ascii/de430/ascp{:04d}.430'.format(year),
            'destination': './data/ascp{:04d}.430'.format(year),
            'force_refresh': refresh
        })
    
    # Download each file
    success_count = 0
    total_files = len(required_files)
    
    print(f"Preparing to download {total_files} files...")
    
    for file_info in required_files:
        if download_file(**file_info):
            success_count += 1
    
    print(f"Downloaded {success_count} of {total_files} files successfully.")

if __name__ == "__main__":
    main()