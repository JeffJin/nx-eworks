import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {CustomerDto, GroupDto} from '../../../models/dtos';
import {CreateCustomerComponent} from './create-customer/create-customer.component';
import {EditCustomerComponent} from './edit-customer/edit-customer.component';
import {CustomerService} from '../../../services/customer.service';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {TableData} from '@app/models/table-data';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';

@Component({
  selector: 'app-customers',
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class CustomersComponent implements OnInit {
  customers: any[];
  keywords = '';
  selectedFilter = 'name';
  tableData: TableData;

  constructor(private customerService: CustomerService, public dialog: MatDialog, public snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
    this.loadCustomers();
  }

  search(key): void {
    this.customerService.searchCustomers(key).subscribe(results => {
      this.customers = results.map(v => {
        v.checked = false;
        return v;
      });
    });
  }

  addNew(): void {
    const dto = new CustomerDto(
      {
        name: '',
        createdOn: new Date()
      }
    );
    const dialogRef = this.dialog.open(CreateCustomerComponent, {
      width: '360px',
      data: dto
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        this.customerService.addCustomer(data).subscribe(results => {
            this.loadCustomers();
          },
          error => {
            this.snackBar.open('Failed to register new device ', 'FAILURE', {
              duration: 5000,
              panelClass: ['action-error']
            });
          });
      }
    });
  }

  loadCustomers(): void {
    this.customerService.getCustomers().subscribe(results => {
      console.log('load group results', results);
      // refresh the result
      this.customers = results.map(v => {
        v.checked = false;
        return v;
      });

      this.tableData = {
        title: 'All Customers',
        template: 'locationTemplate',
        headerRow: ['Name', 'Created On'],
        dataRows: this.customers
      };
    });
  }

  edit(dto: any): void {
    const dialogRef = this.dialog.open(EditCustomerComponent, {
      width: '360px',
      data: dto
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.id) {
        for (let i = 0; i < this.customers.length; i++) {
          if (this.customers[i].id === data.id) {
            this.customers[i] = data;
          }
        }
      }
    });
  }

  remove(dto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.name, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        this.customerService.deleteCustomer(dto.id).subscribe(result => {
          // refresh the result
          this.loadCustomers();
          this.snackBar.open('Customer with name \'' + dto.name + '\' has been successfully removed.', 'SUCCESS', {
            duration: 3000,
            panelClass: ['action-success']
          });
        }, (err) => {
          console.error(err);
          this.snackBar.open('Failed to remove customer with name \'' + dto.name + '\'', 'FAILURE', {
            duration: 5000,
            panelClass: ['action-error']
          });
        });
      }
    });
  }
}
