import {createAction, props} from '@ngrx/store';
import {ImageDto, AudioDto, VideoDto, CategoryDto} from '@app/models/dtos';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';

export const saveVideo = createAction(
  '[videoService] SAVE VIDEO',
  props<{ video: VideoDto }>()
);

export const saveVideoFailure = createAction(
  '[videoService] SAVE VIDEO FAILURE',
  props<{ video: VideoDto }>()
);

export const loadCategories = createAction(
  '[commonService] LOAD CATEGORIES'
);
export const loadImages = createAction(
  '[imageService] LOAD IMAGES',
  props<{ category: string }>()
);
export const loadVideos = createAction(
  '[videoService] LOAD VIDEOS',
  props<{ category: string }>()
);
export const loadVideosSuccess = createAction(
  '[videoService] LOAD VIDEOS SUCCESS',
  props<{ videos: VideoDto[] }>()
);
export const loadVideosFailure = createAction(
  '[videoService] LOAD VIDEOS FAILURE',
  props<{ error: string }>()
);
export const deleteVideo = createAction(
  '[videoService] DELETE VIDEO',
  props<{ id: string }>()
);
export const deleteVideoSuccess = createAction(
  '[videoService] DELETE VIDEO SUCCESS',
  props<{ id: string }>()
);
export const deleteVideoFailure = createAction(
  '[videoService] DELETE VIDEO FAILURE',
  props<{ id: string }>()
);
export const loadAudios = createAction(
  '[audioService] LOAD AUDIOS',
  props<{ category: string }>()
);

export const loadImagesSuccess = createAction(
  '[imageService] LOAD IMAGES SUCCESS',
  props<{ images: ImageDto[] }>()
);
export const loadImagesFailure = createAction(
  '[imageService] LOAD IMAGES FAILURE',
  props<{ error: string }>()
);

export const deleteImageSuccess = createAction(
  '[imageService] DELETE IMAGE SUCCESS',
  props<{ id: string }>()
);
export const deleteImageFailure = createAction(
  '[imageService] DELETE IMAGE FAILURE',
  props<{ error: string }>()
);
export const loadCategoriesFailure = createAction(
  '[commonService] LOAD CATEGORIES FAILURE',
  props<{ error: string }>()
);
export const loadAudiosSuccess = createAction(
  '[audioService] LOAD AUDIOS SUCCESS',
  props<{ audios: AudioDto[] }>()
);

export const loadVideoDetails = createAction(
  '[videoService] LOAD VIDEO DETAILS',
  props<{ videoId: string }>()
);

export const loadAudioDetails = createAction(
  '[audioService] LOAD AUDIO DETAILS',
  props<{ audioId: string }>()
);

export const loadImageDetails = createAction(
  '[imageService] LOAD IMAGE DETAILS',
  props<{ imageId: string }>()
);

export const loadCategoriesSuccess = createAction(
  '[commonService] LOAD CATEGORIES SUCCESS',
  props<{ categories: CategoryDto[] }>()
);

export const loadVideoDetailsSuccess = createAction(
  '[videoService] LOAD VIDEO DETAILS SUCCESS',
  props<{ video: VideoDto }>()
);
export const loadVideoDetailsFailure = createAction(
  '[videoService] LOAD VIDEO DETAILS FAILURE',
  props<{ error: string }>()
);
export const loadImageDetailsSuccess = createAction(
  '[imageService] LOAD IMAGE DETAILS SUCCESS',
  props<{ image: ImageDto }>()
);
export const loadImageDetailsFailure = createAction(
  '[imageService] LOAD IMAGE DETAILS FAILURE',
  props<{ error: string }>()
);
export const addImage = createAction(
  '[imageService] ADD IMAGE',
  props<{ image: ImageDto }>()
);

export const removeImage = createAction(
  '[imageService] REMOVE IMAGE',
  props<{ imageId: string }>()
);

export const updateImage = createAction(
  '[imageService] UPDATE IMAGE',
  props<{ image: ImageDto }>()
);
