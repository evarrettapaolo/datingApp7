import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { UserParams } from '../_models/userParams';

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

  login() {
    this.accountService.login(this.model).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
        this.model = {}; //clears out the fields
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
