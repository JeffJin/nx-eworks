import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {Utils} from '../../../../../services/utils';
import {FileService} from '../../../../../services/file.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {DomSanitizer} from '@angular/platform-browser';
import {ImageService} from '../../../../../services/image.service';
import { combineLatest } from 'rxjs';
import {VideoService} from '../../../../../services/video.service';
import {CommonService} from '@app/services/common.service';

@Component({
  selector: 'app-edit-image',
  templateUrl: './edit-image.component.html',
  styleUrls: ['./edit-image.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class EditImageComponent implements OnInit {
  dto: any;
  categories: any[] = [];
  selectedCategory = null;

  constructor(private utils: Utils, private imageService: ImageService,  private videoService: VideoService,
              private commonService: CommonService, private sanitizer: DomSanitizer,
              public dialogRef: MatDialogRef<EditImageComponent>, @Inject(MAT_DIALOG_DATA) public data: any) { }


  ngOnInit(): void {

    this.dto = this.data.dto;

    combineLatest(this.imageService.getImage(this.dto.id), this.commonService.categories$).subscribe(
      results => {
        this.dto = results[0];
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
    this.imageService.updateImage(this.dto.id, this.dto).subscribe(result => {
        this.dialogRef.close(result);
    });
  }
}
