import { createRouter, createWebHistory } from 'vue-router'

import IndexView from '@/views/index/index.vue';
import BookView from '@/views/book/book.vue';
import BorrowView from '@/views/borrow/borrow.vue'
import CategoryView from '@/views/category/category.vue';
import UserView from '@/views/user/user.vue';
import LoginView from '@/views/login/login.vue';
import LayoutView from '@/views/layout/index.vue';
import BookReportView from '@/views/report/book/index.vue'
import BorrowReportView from '@/views/report/borrow/index.vue'
import CategoryReportView from '@/views/report/category/index.vue'

const routes = [
  // 根路径直接重定向到登录页
  {
    path: '/',
    redirect: '/login',
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: LayoutView,
    redirect: '/index',
    meta: { requiresAuth: true },
    children: [
      { path: 'index', component: IndexView },
      { path: 'book', component: BookView },
      { path: 'borrow', component: BorrowView },
      { path: 'category', component: CategoryView },
      {path: 'user', component: UserView},
      { path: 'report/book', component: BookReportView },
      { path: 'report/borrow', component: BorrowReportView },
      { path: 'report/category', component: CategoryReportView },
    ]
  },
  { 
    path: '/login', 
    component: LoginView,
    meta: { requiresAuth: false }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// ========== 全局路由守卫 ==========
router.beforeEach((to, from, next) => {
  const isLoggedIn = !!localStorage.getItem('loginUser');
  
  console.log('路由守卫:', { to: to.path, isLoggedIn, requiresAuth: to.meta.requiresAuth })

  // 如果访问根路径且未登录，直接到登录页
  if (to.path === '/' && !isLoggedIn) {
    console.log('访问根路径且未登录，直接到登录页')
    next('/login');
  }
  // 需要认证但未登录
  else if (to.meta.requiresAuth && !isLoggedIn) {
    console.log('需要认证但未登录，跳转到登录页')
    next({
      path: '/login',
      query: { redirect: to.fullPath }
    });
  } 
  // 已登录但访问登录页
  else if (to.path === '/login' && isLoggedIn) {
    console.log('已登录访问登录页，跳转到首页')
    next('/index');
  } 
  // 正常访问
  else {
    console.log('正常访问')
    next();
  }
});

export default router;