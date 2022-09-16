import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {Component, Inject} from '@angular/core';

@Component({
  selector: 'app-add-pip-dialog',
  templateUrl: 'add-pip.html',
  styleUrls: ['./add-pip.scss']
})
export class AddPipDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<AddPipDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

}
