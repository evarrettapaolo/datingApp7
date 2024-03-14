import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MembersService } from '../_services/members.service';
import { UserParams } from '../_models/userParams';
import { Member } from '../_models/member';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  userParams: UserParams | undefined; //used to clear request params after logout, not working yet


  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) {
  }

  ngOnInit(): void {
  }

  login(form: NgForm) {
    this.accountService.login(this.model).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
        form.reset();
      },
      error: () => {
        this.toastr.error('invalid login');
      }
    })
  }

  logout() {
    this.accountService.logout();
    // this.userParams = this.memberService.resetUserParams(); 
    this.router.navigateByUrl('/');
  }

}
