ffmpeg -loop 1 -i image.jpg -c:v libx264 -t 15 -pix_fmt yuv420p -vf scale=320:240 out.mp4

ffmpeg -r 60 -f image2 -s 1280x720 -i pic%05d.png -i MP3FILE.mp3 -vcodec libx264 -b 4M -vpre normal -acodec copy OUTPUT.mp4


//capture Facetime camera video
ffmpeg 
  -f avfoundation // global options
  -video_size 1280x720 -framerate 30 //input options
  -i "0" 
  -vcodec libx264 -preset veryfast -f flv //output options
  webcam.mp4

// add text to video
ffmpeg -i sample.mp4 -vf drawtext="fontfile=/Users/Jeff/Projects/files/fonts/arial.ttf:text='this is the text to be placed at bottom':x=w-t*50:y=h-th:fontcolor=blue:fontsize=30" -t 10 output.mp4

//add audio to image
ffmpeg -loop 1 -i input.jpg -i audio.wav -acodec aac -vcodec mpeg4 -t 10 mpeg4.mp4