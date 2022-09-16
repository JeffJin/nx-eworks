import {ChangeDetectionStrategy, Component, OnDestroy, OnInit, ViewEncapsulation} from '@angular/core';
import {VideoService} from '@app/services/video.service';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Observable} from 'rxjs/Observable';
import {CategoryDto, VideoDto} from '@app/models/dtos';
import {Store} from '@ngrx/store';
import {EditVideoComponent} from '@app/modules/dashboard/assets/videos/edit-video/edit-video.component';
import {deleteVideo, loadVideos} from '@app/modules/dashboard/assets/states/assets.actions';
import {
  selectCategories,
  selectDeleteVideoFailure,
  selectDeleteVideoSuccess,
  selectMyVideos
} from '@app/modules/dashboard/assets/states/assets.reducers';
import {catchError, debounceTime, map, takeUntil, tap} from 'rxjs/operators';
import {selectQueryParams} from '@app/states/router.selectors';
import {Subscription} from 'rxjs/Subscription';
import {EMPTY} from 'rxjs';
import {Subject} from 'rxjs/Subject';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';

@Component({
  selector: 'app-videos-shell',
  templateUrl: './video-list-shell.component.html',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VideoListShellComponent implements OnInit {
  videos$: Observable<VideoDto[]> | null;
  categories$: Observable<CategoryDto[]> | null;
  selectedFilter$: Observable<string>;
  private errorSubject = new Subject<string>();
  errorMessage$ = this.errorSubject.asObservable();

  constructor(
    private videoService: VideoService,
    private store: Store,
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
  ) {
    this.selectedFilter$ = this.store.select(selectQueryParams).pipe(
      debounceTime(50),
      tap(params => console.log('video shell component query params changed', params)),
      map(queryParams => {
        this.store.dispatch(loadVideos({category: queryParams.category}));
        return queryParams.category;
      })
    );
  }

  ngOnInit(): void {
    this.categories$ = this.store.select(selectCategories);
    this.videos$ = this.store.select(selectMyVideos).pipe(
      catchError(err => {
        this.errorSubject.next(err);
        this.snackBar.open(err, 'FAILURE', {
          duration: 3000,
          panelClass: ['action-error']
        });
        return EMPTY;
      })
    );
  }
}
