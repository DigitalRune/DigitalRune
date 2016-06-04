$thumbnailWidth = 120  # Max width of the thumbnail image.
$thumbnailHeight = 120 # Max height of the thumbnail image.
$thumbnailQuality = 75 # JPEG quality [0, 100]


function CreateThumbnail
{
    param ([System.IO.FileInfo]$fileInfo, [System.Int32]$maxWidth, [System.Int32]$maxHeight, [System.Int32]$quality)

    # Skip thumbnail image.
    if ($fileInfo.Name.IndexOf("thumb", [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
    {
        return
    }

    # Change "path/file.ext" to "path/file-thumb.jpg".
    $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($fileInfo.Name)
    $thumbFileName = Join-Path $fileInfo.DirectoryName "$fileNameWithoutExtension-thumb.jpg"

    Write-Host "$fileInfo -> $thumbFileName"

    # Encoder parameter for image quality.
    $encoder = [System.Drawing.Imaging.Encoder]::Quality
    $encoderParameters = New-Object System.Drawing.Imaging.EncoderParameters(1)
    $encoderParameters.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter($encoder, $quality)

    # Codec
    $imageCodecInfo = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }

    # Load image
    $image = [System.Drawing.Image]::FromFile($fileInfo.FullName, $true);

    # Determine factor to maintain original aspect ratio.
    $ratioX = $maxWidth / $image.Width;
    $ratioY = $maxHeight / $image.Height;
    $ratio = $ratioY
    if($ratioX -le $ratioY)
    {
      $ratio = $ratioX
    }

    # Determine thumbnail size.
    $width = [int]($image.Width * $ratio)
    $height = [int]($image.Height * $ratio)

    $thumb = $image.GetThumbnailImage($width, $height, $null, [IntPtr]::Zero);
    $thumb.Save($thumbFileName, $imageCodecInfo, $encoderParams);
    $image.Dispose();
    $thumb.Dispose();   
}

Get-ChildItem *.jpg, *.png | ForEach-Object { 
    CreateThumbnail $_ $thumbnailWidth $thumbnailHeight $thumbnailQuality 
}
