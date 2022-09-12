import {VideoService} from '@app/services/video.service';
import {Injectable} from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import {of} from 'rxjs';
import {map, catchError, switchMap, exhaustMap, tap, withLatestFrom, debounceTime} from 'rxjs/operators';
import {
  deleteVideo, deleteVideoFailure, deleteVideoSuccess,
  loadVideoDetails,
  loadVideoDetailsFailure,
  loadVideoDetailsSuccess,
  loadVideos,
  loadVideosFailure,
  loadVideosSuccess, saveVideo, saveVideoFailure,
} from '../states/assets.actions';
import {select, Store} from '@ngrx/store';
import {selectQueryParam, selectQueryParams} from '@app/states/router.selectors';
import {selectCategories} from '@app/modules/dashboard/assets/states/assets.reducers';

@Injectable()
export class VideoEffects {
  constructor(
    private store: Store,
    private actions$: Actions,
    private videoService: VideoService
  ) {}

  loadVideos$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadVideos),
      debounceTime(500),
      map(action => action.category),
      switchMap((category) => {
        // console.log('loadVideos$', category);
        return this.videoService.getVideos(category)
          .pipe(
            map(videos => {
              const results = videos.map(v => {
                v.checked = false;
                v.assetType = 'Video';
                return v;
              });
              return loadVideosSuccess({videos: results});
            }),
            catchError((error) => {
              // TODO produce custom error messages
              return of(loadVideosFailure({error: 'Failed to load videos'}));
            })
          );
        }
      )
    )
  );

  loadVideoDetails$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadVideoDetails),
      map(action => action.videoId),
      switchMap(videoId =>
        this.videoService.getVideo(videoId).pipe(
          map(video => {
            return loadVideoDetailsSuccess({video});
          }),
          catchError((error) => of(loadVideoDetailsFailure({error})))
        )
      )
    )
  );

  deleteVideo$ = createEffect(() =>
    this.actions$.pipe(
      ofType(deleteVideo),
      map(action => action.id),
      exhaustMap(id =>
        this.videoService.deleteVideo(id).pipe(
          map(video => {
            return deleteVideoSuccess({id});
          }),
          catchError((error) => of(deleteVideoFailure({id})))
        )
      )
    )
  );

  saveVideo$ = createEffect(() =>
    this.actions$.pipe(
      ofType(saveVideo),
      map(action => action.video),
      exhaustMap(video =>
        this.videoService.updateVideo(video.id, video).pipe(
          map(videoUpdated => {
            return loadVideoDetailsSuccess({video: videoUpdated});
          }),
          catchError((error) => of(saveVideoFailure({video})))
        )
      )
    )
  );

}
