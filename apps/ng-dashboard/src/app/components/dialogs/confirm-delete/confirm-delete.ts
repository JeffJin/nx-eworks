import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {Component, EventEmitter, Inject, Input, Output, ViewEncapsulation} from '@angular/core';

@Component({
  selector: 'app-confirm-delete-dialog',
  templateUrl: 'confirm-delete.html',
  styleUrls: ['./confirm-delete.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class ConfirmDeleteDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDeleteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

  cancel(): void {
    this.dialogRef.close();
  }

  continue(): void{
    this.data.result = 'continue';
  }

}
