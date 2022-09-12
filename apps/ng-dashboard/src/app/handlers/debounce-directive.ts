/**
 * Debounce a method
 */
import {debounce} from 'lodash';

export const Debounce = ms => (target: any, key: any, descriptor: any) => {
  const oldFunc = descriptor.value;
  const newFunc = debounce(oldFunc, ms);
  // tslint:disable-next-line:typedef
  descriptor.value = function() {
    return newFunc.apply(this, arguments);
  };
};
