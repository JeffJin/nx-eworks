import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'app-create-customer',
  templateUrl: './create-customer.component.html',
  styleUrls: ['./create-customer.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class CreateCustomerComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<CreateCustomerComponent>,
              @Inject(MAT_DIALOG_DATA) public data: any) { }

  ngOnInit(): void {
  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(data){
    this.dialogRef.close(data);
  }
}
