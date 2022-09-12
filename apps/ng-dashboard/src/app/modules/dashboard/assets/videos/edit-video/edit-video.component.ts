import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {FileService} from '@app/services/file.service';
import {Utils} from '@app/services/utils';
import {DomSanitizer, SafeResourceUrl} from '@angular/platform-browser';
import {VideoService} from '@app/services/video.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import { combineLatest } from 'rxjs';
import {CommonService} from '@app/services/common.service';

@Component({
  selector: 'app-edit-video',
  templateUrl: './edit-video.component.html',
  styleUrls: ['./edit-video.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class EditVideoComponent implements OnInit {
  videoDto: any;
  thumbnails: string[] = [];
  categories: any[] = [];
  sanitizedVodUrl: SafeResourceUrl;
  selectedCategory = null;

  constructor(private utils: Utils, private videoService: VideoService, private fileService: FileService,
              private commonService: CommonService, private sanitizer: DomSanitizer,  public dialogRef: MatDialogRef<EditVideoComponent>,
              @Inject(MAT_DIALOG_DATA) public data: any) {

  }

  ngOnInit(): void {

    this.videoDto = this.data.dto;
    this.sanitizedVodUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.data.dto.cloudUrl);

    combineLatest(this.videoService.getVideo(this.videoDto.id), this.commonService.categories$).subscribe(
      results => {
        this.videoDto = results[0];
        this.sanitizedVodUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.videoDto.cloudUrl);

        this.categories = results[1];
        this.selectedCategory = this.categories.find(cat => cat.name === this.videoDto.category);
      }
    );

    this.videoService.getThumbnails(this.videoDto.id).subscribe(results => {
      this.thumbnails = results;
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(): void {
    this.videoDto.category = this.selectedCategory.name;
    this.videoService.updateVideo(this.videoDto.id, this.videoDto).subscribe(result => {
        this.dialogRef.close(result);
    });
  }

}
