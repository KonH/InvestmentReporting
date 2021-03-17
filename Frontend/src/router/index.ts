import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import Home from '@/views/home.vue';
import Login from '@/views/login.vue';
import Register from '@/views/register.vue';

const routes: Array<RouteRecordRaw> = [
	{
		path: '/',
		name: 'Home',
		component: Home,
	},
	{
		path: '/login',
		name: 'Login',
		component: Login,
		meta: {
			guest: true,
		},
	},
	{
		path: '/register',
		name: 'Register',
		component: Register,
		meta: {
			guest: true,
		},
	},
];

const router = createRouter({
	history: createWebHistory(),
	routes,
});

router.beforeEach((to, from, next) => {
	const guest = to.meta.guest as boolean;
	if (guest) {
		next();
	} else {
		// TODO: proper auth check
		console.log('redirect to login');
		next({ path: '/login' });
	}
});

export default router;
