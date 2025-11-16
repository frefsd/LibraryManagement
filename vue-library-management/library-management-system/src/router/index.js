import { createRouter, createWebHistory } from 'vue-router'

import IndexView from '@/views/index/index.vue';
import BookView from '@/views/book/book.vue';
import BorrowView from '@/views/borrow/borrow.vue'
import LoginView from '@/views/login/login.vue';
import LayoutView from '@/views/layout/index.vue';
import CategoryView from '@/views/category/category.vue';
import BookReportView from '@/views/report/book/index.vue'
import BorrowReportView from '@/views/report/borrow/index.vue'
import CategoryReportView from '@/views/report/category/index.vue'

const routes = [
  {
    path: '/',
    component: LayoutView, //布局容器
    redirect: '/index', //重定向
    children: [
      { path: '/index', component: IndexView },
      { path: '/book', component: BookView },
      { path: '/borrow', component: BorrowView },
      { path: '/category', component: CategoryView },
      { path: '/report/book', component: BookReportView },
      { path: '/report/borrow', component: BorrowReportView },
      { path: '/report/category', component: CategoryReportView },

    ]
  },
  { path: '/login', component: LoginView }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
