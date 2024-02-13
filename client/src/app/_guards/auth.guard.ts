import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  //DI objects
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  //check user object if available
  return accountService.currentUser$.pipe(
    map(user => {
      if(user) return true;
      else {
        toastr.error('invalid access');
        return false;
      }
    })
  )
};
