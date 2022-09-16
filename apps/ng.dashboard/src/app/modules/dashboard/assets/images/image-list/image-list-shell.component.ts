import {Component, OnDestroy, OnInit, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {EditImageComponent} from '../edit-image/edit-image.component';
import {Store} from '@ngrx/store';
import {CategoryDto, ImageDto} from '@app/models/dtos';
import {Observable} from 'rxjs/Observable';
import {
  selectCategories,
  selectDeleteImageFailure,
  selectDeleteImageSuccess,
  selectMyImages
} from '@app/modules/dashboard/assets/states/assets.reducers';
import {loadImages, removeImage} from '../../states/assets.actions';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {catchError, debounceTime, map, takeUntil, tap} from 'rxjs/operators';
import {Subject} from 'rxjs/Subject';
import {selectQueryParams} from '@app/states/router.selectors';
import {EMPTY} from 'rxjs';

@Component({
  selector: 'app-images-shell',
  templateUrl: './image-list-shell.component.html',
  styleUrls: ['./image-list-shell.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class ImageListShellComponent implements OnInit {
  images$: Observable<ImageDto[]> | null;
  categories$: Observable<CategoryDto[]> | null;
  selectedFilter$: Observable<string>;
  private errorSubject = new Subject<string>();
  errorMessage$ = this.errorSubject.asObservable();

  constructor(private store: Store,
              public dialog: MatDialog,
              public snackBar: MatSnackBar) {
    this.selectedFilter$ = this.store.select(selectQueryParams).pipe(
      debounceTime(50),
      // takeUntil(this.ngDestroyed$),
      tap(params => console.log('image shell component query params changed', params)),
      map(queryParams => {
        this.store.dispatch(loadImages({category: queryParams.category}));
        return queryParams.category;
      })
    );
  }

  ngOnInit(): void {
    this.categories$ = this.store.select(selectCategories);
    this.images$ = this.store.select(selectMyImages).pipe(
      catchError(err => {
        this.errorSubject.next(err);
        this.snackBar.open(err, 'FAILURE', {
          duration: 3000,
          panelClass: ['action-error']
        });
        return EMPTY;
      })
    );
    this.store.select(selectDeleteImageSuccess).subscribe((payload) => {
      this.snackBar.open('Image with title \'' + payload.item.title + '\' has been successfully removed.', 'SUCCESS', {
        duration: 3000,
        panelClass: ['action-success']
      });
    });
    this.store.select(selectDeleteImageFailure).subscribe((payload) => {
      this.snackBar.open('Failed to remove image with title \'' + payload.item.title + '\'', 'FAILURE', {
        duration: 5000,
        panelClass: ['action-error']
      });
    });
  }

  remove(dto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.title, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        this.store.dispatch(removeImage(dto.id));
      }
    });
  }

  edit(dto): void {
    const dialogRef = this.dialog.open(EditImageComponent, {
      width: '700px',
      data: {dto}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        // for (let i = 0; i < images.length; i++) {
        //   if (images[i].id === data.id) {
        //     images[i] = data;
        //   }
        // }
      }
    });
  }
}
