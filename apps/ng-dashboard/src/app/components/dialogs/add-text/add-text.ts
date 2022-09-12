import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {Component, Inject} from '@angular/core';

@Component({
  selector: 'app-add-text-dialog',
  templateUrl: 'add-text.html',
  styleUrls: ['./add-text.scss']
})
export class AddTextDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<AddTextDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

}
