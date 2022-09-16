import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {Utils} from '../../../../../services/utils';
import { combineLatest } from 'rxjs';
import {DomSanitizer, SafeResourceUrl} from '@angular/platform-browser';
import {FileService} from '../../../../../services/file.service';
import {AudioService} from '../../../../../services/audio.service';
import {VideoService} from '../../../../../services/video.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {CommonService} from '@app/services/common.service';

@Component({
  selector: 'app-edit-audio',
  templateUrl: './edit-audio.component.html',
  styleUrls: ['./edit-audio.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class EditAudioComponent implements OnInit {
  dto: any;
  categories: any[] = [];
  sanitizedCloudUrl: SafeResourceUrl;
  selectedCategory = null;

  constructor(private utils: Utils, private audioService: AudioService,  private videoService: VideoService,
              private fileService: FileService, private commonService: CommonService, private sanitizer: DomSanitizer,
              public dialogRef: MatDialogRef<EditAudioComponent>, @Inject(MAT_DIALOG_DATA) public data: any) { }


  ngOnInit(): void {

    this.dto = this.data.dto;
    this.sanitizedCloudUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.data.dto.cloudUrl);

    combineLatest(this.audioService.getAudio(this.dto.id), this.commonService.categories$).subscribe(
      results => {
        this.dto = results[0];
        this.sanitizedCloudUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.dto.cloudUrl);

        this.categories = results[1];
        this.selectedCategory = this.categories.find(cat => cat.name === this.dto.category);
      }
    );

  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(): void{
    this.dto.category = this.selectedCategory.name;
    this.audioService.updateAudio(this.dto.id, this.dto).subscribe(result => {
        this.dialogRef.close(result);
    });
  }
}
