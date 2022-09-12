import {Injectable} from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import {of} from 'rxjs';
import {map, catchError, mergeMap, exhaustMap, tap} from 'rxjs/operators';
import {
  deleteImageFailure,
  deleteImageSuccess,
  loadImageDetails,
  loadImageDetailsFailure,
  loadImageDetailsSuccess,
  loadImages,
  loadImagesFailure,
  loadImagesSuccess,
  removeImage,
} from '../states/assets.actions';
import {ImageService} from '@app/services/image.service';

@Injectable()
export class ImageEffects {
  loadImages$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadImages),
      exhaustMap((action) => this.imageService.getImages(action.category)
        .pipe(
          map(images => {
            const results = images.map(v => {
              v.checked = false;
              v.assetType = 'Image';
              return v;
            });
            return loadImagesSuccess({images: results});
          }),
          catchError((error) => of(loadImagesFailure({error})))
        )
      )
    )
  );

  loadImageDetails$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadImageDetails),
      exhaustMap(action =>
        this.imageService.getImage(action.imageId).pipe(
          map(image => {
            return loadImageDetailsSuccess({image});
          }),
          catchError((error) => of(loadImageDetailsFailure({error})))
        )
      )
    )
  );

  deleteImage$ = createEffect(() =>
    this.actions$.pipe(
      ofType(removeImage),
      exhaustMap(action =>
        this.imageService.deleteImage(action.imageId).pipe(
          map(image => {
            return deleteImageSuccess({id: action.imageId});
          }),
          catchError((error) => of(deleteImageFailure({error})))
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private imageService: ImageService
  ) {}
}
