import {createFeatureSelector, createReducer, createSelector, on} from '@ngrx/store';
import {AudioDto, ImageDto, UserDto, VideoDto} from '@app/models/dtos';
import {
  addImage,
  updateImage,
  removeImage,
  loadAudiosSuccess,
  loadVideosSuccess,
  loadImagesSuccess,
  loadVideoDetailsSuccess,
  loadCategoriesSuccess,
  deleteVideoSuccess,
  deleteVideoFailure,
  deleteImageSuccess,
  deleteImageFailure,
  loadVideosFailure, loadImageDetailsSuccess,
} from './assets.actions';
import {AssetsState} from './assets.state';
import {selectUser} from '@app/states/auth/auth.reducers';

export const initialState: AssetsState = {
  metadata: {
    categories: [],
  },
  video: {
    updateVideoFailure: null,
    updateVideoSuccess: null,
    deleteVideoFailure: null,
    deleteVideoSuccess: null,
    videos: [],
    selectedVideo: null,
    error: null,
  },
  image: {
    loadImagesFailure: null,
    updateImageFailure: null,
    updateImageSuccess: null,
    deleteImageFailure: null,
    deleteImageSuccess: null,
    images: [],
    selectedImage: null,
    error: null,
  },
  audio: {
    audios: [],
    selectedAudio: null,
  },
};

const getAssetsFeatureState = createFeatureSelector<AssetsState>('assets');

export const selectImages = createSelector(
  getAssetsFeatureState,
  state => state.image.images
);

export const selectVideos = createSelector(
  getAssetsFeatureState,
  state => state.video.videos
);

export const selectVideoDetails = createSelector(
  getAssetsFeatureState,
  state => state.video.selectedVideo
);

export const selectDeleteVideoSuccess = createSelector(
  getAssetsFeatureState,
  state => state.video.deleteVideoSuccess
);

export const selectDeleteVideoFailure = createSelector(
  getAssetsFeatureState,
  state => state.video.deleteVideoFailure
);

export const selectUpdateVideoSuccess = createSelector(
  getAssetsFeatureState,
  state => state.video.updateVideoSuccess
);

export const selectUpdateVideoFailure = createSelector(
  getAssetsFeatureState,
  state => state.video.updateVideoFailure
);

export const selectAudios = createSelector(
  getAssetsFeatureState,
  state => state.audio.audios
);

export const selectCategories = createSelector(
  getAssetsFeatureState,
  state => state.metadata.categories
);

export const selectMyImages = createSelector(
  selectUser,
  selectImages,
  (selectedUser: UserDto, images: ImageDto[]) => {
    if (selectedUser && images) {
      return images.filter((img: ImageDto) => img.createdBy === selectedUser.email);
    } else {
      return [];
    }
  }
);

export const selectImageDetails = createSelector(
  getAssetsFeatureState,
  state => state.image.selectedImage
);

export const selectDeleteImageSuccess = createSelector(
  getAssetsFeatureState,
  state => state.image.deleteImageSuccess
);

export const selectDeleteImageFailure = createSelector(
  getAssetsFeatureState,
  state => state.image.deleteImageFailure
);

export const selectMyVideos = createSelector(
  selectVideos,
  (videos: VideoDto[]) => {
    if (videos) {
      return videos;
    } else {
      return [];
    }
  }
);

export const selectMyAudios = createSelector(
  selectUser,
  selectAudios,
  (selectedUser: UserDto, audios: AudioDto[]) => {
    if (selectedUser && audios) {
      return audios.filter((audio: ImageDto) => audio.createdBy === selectedUser.email);
    } else {
      return [];
    }
  }
);

export const assetReducer = createReducer(
  initialState,
  on(loadCategoriesSuccess, (state, { categories }) => {
    return {...state, metadata: {categories}};
  }),
  on(loadVideosSuccess, (state, { videos }) => {
      return ({...state, video: {...state.video, videos}});
    }
  ),
  on(loadVideosFailure, (state, { error }) => ({...state, video: {...state.video, videos: [], error}})),
  on(loadVideoDetailsSuccess, (state, { video }) => {
    console.log('loadVideoDetailsSuccess', video);
    // TODO update the existing video list
    return {...state, video: {...state.video, selectedVideo: video }};
  }),
  on(deleteVideoSuccess, (state, { id }) => {
    console.log('deleteVideoSuccess', id);
    return {...state, video: {...state.video, videos: state.video.videos.filter(i => i.id !== id)}};
  }),
  on(deleteVideoFailure, (state, { id }) => {
    console.log('deleteVideoFailure', id);
    return state;
  }),
  on(deleteImageSuccess, (state, { id }) => {
    console.log('deleteImageSuccess', id);
    return {...state, image: {...state.image, images: state.image.images.filter(i => i.id !== id)}};
  }),
  on(deleteImageFailure, (state, { error }) => {
    console.log('deleteImageFailure', error);
    return state;
  }),
  on(loadImagesSuccess, (state, { images }) => ({...state, image: {...state.image, images}})),
  on(loadImageDetailsSuccess, (state, { image }) => {
    return {...state, image: {...state.image, selectedImage: image }};
  }),
  on(addImage, (state, { image }) => {
    if (state.image.images.find(i => i.id === image.id)) {
      return state;
    }
    return {...state, image: {...state.image, images: [...state.image.images, image]}};
  }),
  on(updateImage, (state, { image }) => {
    const img = state.image.images.find(i => i.id === image.id);
    if (!img) {
      return state;
    }
    return {...state, image: {...state.image, images: [...state.image.images.filter(i => i.id !== image.id), image]}};
  }),
  on(removeImage, (state, { imageId }) => {
    return {...state, image: {...state.image, images: state.image.images.filter(i => i.id !== imageId)}};
  }),
  on(loadAudiosSuccess, (state, { audios }) => ({...state, audios})),
);

// export const assetsReducerMap = {
//   metadata: metadataReducer,
//   video: videoReducer,
//   image: imageReducer,
//   audio: audioReducer,
// };


