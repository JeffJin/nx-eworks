import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Store} from '@ngrx/store';
import {ActivatedRoute} from '@angular/router';
import {loadVideoDetails} from '@app/modules/dashboard/assets/states/assets.actions';
import {selectVideoDetails} from '@app/modules/dashboard/assets/states/assets.reducers';
import {VideoDto} from '@app/models/dtos';
import {EditVideoComponent} from '@app/modules/dashboard/assets/videos/edit-video/edit-video.component';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';

@Component({
  selector: 'app-video-details',
  templateUrl: './video-details.component.html',
  styleUrls: ['./video-details.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class VideoDetailsComponent implements OnInit {
  video: VideoDto;
  options: {
    fluid?: boolean,
    aspectRatio?: string,
    autoplay?: boolean,
    controls?: boolean,
    sources: {
      src: string,
      type: string,
    }[],
    title?: string,
    desc?: string,
    createdOn?: Date,
  } = {
    fluid: true,
    aspectRatio: '16:9',
    autoplay: true,
    controls: true,
    sources: [{
      src: '',
      type: 'video/mp4',
    }],
    title: '',
    desc: '',
    createdOn: new Date(),
  };

  constructor(
    private store: Store,
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
    private route: ActivatedRoute
  ) {
    this.route.queryParams.subscribe(params => {
      const videoId = params.vid;
      const parts = [];
      parts.push(videoId.slice(0, 8));
      parts.push(videoId.slice(8, 12));
      parts.push(videoId.slice(12, 16));
      parts.push(videoId.slice(16, 20));
      parts.push(videoId.slice(20, 32));
      const videoGuid = parts.join('-');
      console.log('this.videoId', videoGuid);

      // find video url from videoId
      this.store.dispatch(loadVideoDetails({videoId: videoGuid}));
      setTimeout( () => {
        const videoObs = this.store.select(selectVideoDetails);
        videoObs.subscribe(result => {
          this.video = result;
          console.log('result videoId, cloudUrl', result.id, result.cloudUrl);

          if (this.video && this.video.cloudUrl) {
            this.options.sources[0].src = this.video.cloudUrl;
          }
        });
      }, 512);
    });
  }

  ngOnInit(): void {
  }


  edit(dto): void {
    const dialogRef = this.dialog.open(EditVideoComponent, {
      width: '700px',
      data: {dto}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        // TODO publish the data change
      }
    });
  }

  remove(dto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.title, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        // TODO dispatch a delete action
      }
    });
  }
}
