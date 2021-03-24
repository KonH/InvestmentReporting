import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import Home from '@/views/home.vue';
import Login from '@/views/login.vue';
import Register from '@/views/register.vue';
import Backend from '@/service/backend';
import AddBroker from '@/views/addBroker.vue';
import AddCurrency from '@/views/addCurrency.vue';

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
	{
		path: '/addBroker',
		name: 'Add Broker',
		component: AddBroker,
	},
	{
		path: '/addCurrency',
		name: 'Add Currency',
		component: AddCurrency,
	},
];

export const router = createRouter({
	history: createWebHistory(),
	routes,
});

router.beforeEach(async (to, from, next) => {
	const guest = to.meta.guest as boolean;
	if (guest) {
		next();
	} else {
		const response = await Backend.tryFetch(Backend.auth().check.checkList());
		if (response?.ok) {
			next();
		} else {
			next({ path: '/login' });
		}
	}
});

export default router;
