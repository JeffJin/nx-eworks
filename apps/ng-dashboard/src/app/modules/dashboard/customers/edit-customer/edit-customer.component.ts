import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {CustomerDto} from '../../../../models/dtos';
import {CustomerService} from '../../../../services/customer.service';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {EditGroupDialogComponent} from '@app/components/dialogs/edit-group/edit-group.component';

@Component({
  selector: 'app-edit-customer',
  templateUrl: './edit-customer.component.html',
  styleUrls: ['./edit-customer.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditCustomerComponent implements OnInit {

  constructor(private customerService: CustomerService,
              public dialogRef: MatDialogRef<EditGroupDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: any) { }

  ngOnInit() {
  }

  save(dto: CustomerDto){
    this.customerService.updateCustomer(dto.id, dto).subscribe((result) => {
        this.dialogRef.close(result);
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

}
