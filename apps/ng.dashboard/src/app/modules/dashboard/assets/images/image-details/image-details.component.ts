import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Observable} from 'rxjs/Observable';
import {Store} from '@ngrx/store';
import {loadImageDetails} from '@app/modules/dashboard/assets/states/assets.actions';
import {selectImageDetails, selectVideoDetails} from '@app/modules/dashboard/assets/states/assets.reducers';
import {selectQueryParams} from '@app/states/router.selectors';
import {debounceTime, map, tap} from 'rxjs/operators';

@Component({
  selector: 'app-image-details',
  templateUrl: './image-details.component.html',
  styleUrls: ['./image-details.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class ImageDetailsComponent implements OnInit {
  selectedImage$: Observable<any>;

  constructor(
    private store: Store,
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
  ) {
    console.log('image details component::');
    this.store.select(selectQueryParams).pipe(
      debounceTime(50),
      tap(params => console.log('image details component query params changed', params)),
      map(queryParams => {
        const imageId = queryParams.iid;
        const parts = [];
        parts.push(imageId.slice(0, 8));
        parts.push(imageId.slice(8, 12));
        parts.push(imageId.slice(12, 16));
        parts.push(imageId.slice(16, 20));
        parts.push(imageId.slice(20, 32));
        const imageGuid = parts.join('-');
        console.log('this.imageId', imageGuid);
        return queryParams.iid;
      })
    ).subscribe((imageGuid) => {
      this.store.dispatch(loadImageDetails({imageId: imageGuid}));
    });
  }

  ngOnInit(): void {
    this.selectedImage$ = this.store.select(selectImageDetails);
  }
}
