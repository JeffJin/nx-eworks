export interface Notification {
  createdOn: Date;
  id: string;
  type: string;
  payload: any;
}
export interface TableData {
  title: string;
  template: string;
  headerRow?: string[];
  footerRow?: string[];
  dataRows: any[];
}

