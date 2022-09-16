import {Component, OnDestroy, OnInit, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {EditImageComponent} from './edit-image/edit-image.component';
import {Store} from '@ngrx/store';
import {CategoryDto, ImageDto} from '@app/models/dtos';
import {Observable} from 'rxjs/Observable';
import {
  selectCategories,
  selectDeleteImageFailure,
  selectDeleteImageSuccess,
  selectMyImages
} from '@app/modules/dashboard/assets/states/assets.reducers';
import {loadImages, removeImage} from '../states/assets.actions';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {catchError, debounceTime, map, takeUntil, tap} from 'rxjs/operators';
import {Subject} from 'rxjs/Subject';
import {selectQueryParams} from '@app/states/router.selectors';
import {EMPTY} from 'rxjs';

@Component({
  selector: 'app-images',
  templateUrl: './image-shell.component.html',
  encapsulation: ViewEncapsulation.Emulated
})
export class ImageShellComponent implements OnInit {
  private errorSubject = new Subject<string>();
  errorMessage$ = this.errorSubject.asObservable();

  constructor(private store: Store,
              public dialog: MatDialog,
              public snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
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
