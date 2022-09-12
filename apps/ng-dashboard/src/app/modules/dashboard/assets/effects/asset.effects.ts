import {Injectable} from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import {of} from 'rxjs';
import {map, catchError, mergeMap, exhaustMap, tap} from 'rxjs/operators';
import {CommonService} from '@app/services/common.service';
import {loadCategories, loadCategoriesFailure, loadCategoriesSuccess} from '../states/assets.actions';

@Injectable()
export class AssetEffects {
  loadCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadCategories),
      exhaustMap(() => this.commonService.categories$
        .pipe(
          map(categories => {
            return loadCategoriesSuccess({categories});
          }),
          catchError((error) => of(loadCategoriesFailure({error})))
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private commonService: CommonService
  ) {}
}
