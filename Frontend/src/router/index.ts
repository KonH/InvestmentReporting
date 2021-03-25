import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import Home from '@/view/home.vue';
import Login from '@/view/login.vue';
import Register from '@/view/register.vue';
import Backend from '@/service/backend';
import AddBroker from '@/view/addBroker.vue';
import AddCurrency from '@/view/addCurrency.vue';
import AddAccount from '@/view/addAccount.vue';
import AddIncome from '@/view/addIncome.vue';
import AddExpense from '@/view/addExpense.vue';
import Import from '@/view/import.vue';

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
	{
		path: '/addAccount/:broker',
		name: 'Add Account',
		component: AddAccount,
	},
	{
		path: '/addIncome/:broker/:account',
		name: 'Add Income',
		component: AddIncome,
	},
	{
		path: '/addExpense/:broker/:account',
		name: 'Add Expense',
		component: AddExpense,
	},
	{
		path: '/import/:broker',
		name: 'Import',
		component: Import,
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
