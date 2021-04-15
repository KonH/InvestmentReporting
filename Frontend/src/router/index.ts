import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import Login from '@/view/login.vue';
import Register from '@/view/register.vue';
import Backend from '@/service/backend';
import AddBrokerView from '@/view/addBrokerView.vue';
import AddCurrencyView from '@/view/addCurrencyView.vue';
import AddAccount from '@/view/addAccount.vue';
import AddIncome from '@/view/addIncome.vue';
import AddExpense from '@/view/addExpense.vue';
import ImportView from '@/view/importView.vue';
import OperationList from '@/view/operationList.vue';
import BuyAsset from '@/view/buyAsset.vue';
import SellAsset from '@/view/sellAsset.vue';
import AssetOperationList from '@/view/assetOperationList.vue';
import PortfolioView from '@/view/portfolioView.vue';
import ConfigView from '@/view/configView.vue';

const routes: Array<RouteRecordRaw> = [
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
		path: '/broker/:broker/account/new',
		name: 'Add Account',
		component: AddAccount,
	},
	{
		path: '/broker/:broker/account/:account/income',
		name: 'Add Income',
		component: AddIncome,
	},
	{
		path: '/broker/:broker/account/:account/expense',
		name: 'Add Expense',
		component: AddExpense,
	},
	{
		path: '/broker/:broker/account/:account/operations',
		name: 'Operations',
		component: OperationList,
	},
	{
		path: '/broker/:broker/asset/:asset/operations',
		name: 'Asset Operations',
		component: AssetOperationList,
	},
	{
		path: '/broker/:broker/asset/buy',
		name: 'Buy Asset',
		component: BuyAsset,
	},
	{
		path: '/broker/:broker/asset/sell',
		name: 'Sell Asset',
		component: SellAsset,
	},
	{
		path: '/config',
		name: 'Config',
		component: ConfigView,
	},
	{
		path: '/broker/new',
		name: 'Add Broker',
		component: AddBrokerView,
	},
	{
		path: '/currency/new',
		name: 'Add Currency',
		component: AddCurrencyView,
	},
	{
		path: '/import',
		name: 'Import',
		component: ImportView,
	},
	{
		path: '/portfolio',
		alias: '/',
		name: 'Portfolio',
		component: PortfolioView,
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
