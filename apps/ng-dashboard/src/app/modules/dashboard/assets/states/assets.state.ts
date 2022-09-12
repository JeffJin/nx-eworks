import {AudioDto, CategoryDto, ImageDto, VideoDto} from '@app/models/dtos';

export const assetsKey = 'assets';

export interface AssetsState {
  video: {
    updateVideoSuccess: any;
    updateVideoFailure: any;
    deleteVideoSuccess: any;
    deleteVideoFailure: any;
    videos: VideoDto[];
    selectedVideo: VideoDto;
    error: string;
  };
  image: {
    loadImagesFailure: any;
    updateImageSuccess: any;
    updateImageFailure: any;
    deleteImageSuccess: any;
    deleteImageFailure: any;
    images: ImageDto[];
    selectedImage: ImageDto;
    error: string;
  };
  audio: {
    audios: AudioDto[];
    selectedAudio: AudioDto;
  };
  metadata: {
    categories: CategoryDto[];
  };
}
