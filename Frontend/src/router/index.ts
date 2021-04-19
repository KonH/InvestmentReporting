import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import LoginView from '@/view/loginView.vue';
import RegisterView from '@/view/registerView.vue';
import Backend from '@/service/backend';
import AddBrokerView from '@/view/addBrokerView.vue';
import AddAccountView from '@/view/addAccountView.vue';
import AddIncomeView from '@/view/addIncomeView.vue';
import AddExpenseView from '@/view/addExpenseView.vue';
import ImportView from '@/view/importView.vue';
import BuyAssetView from '@/view/buyAssetView.vue';
import SellAssetView from '@/view/sellAssetView.vue';
import ConfigView from '@/view/configView.vue';
import OperationsView from '@/view/operationsView.vue';
import PortfolioView from '@/view/portfolioView.vue';
import TagsView from '@/view/tagsView.vue';
import DashboardsView from '@/view/dashboardsView.vue';
import CustomView from '@/view/customView.vue';

const routes: Array<RouteRecordRaw> = [
	{
		path: '/login',
		name: 'Login',
		component: LoginView,
		meta: {
			guest: true,
		},
	},
	{
		path: '/register',
		name: 'Register',
		component: RegisterView,
		meta: {
			guest: true,
		},
	},
	{
		path: '/config',
		name: 'Config',
		component: ConfigView,
	},
	{
		path: '/broker/:broker/account/new',
		name: 'Add Account',
		component: AddAccountView,
	},
	{
		path: '/broker/new',
		name: 'Add Broker',
		component: AddBrokerView,
	},
	{
		path: '/import',
		name: 'Import',
		component: ImportView,
	},
	{
		path: '/operations',
		name: 'operations',
		component: OperationsView,
	},
	{
		path: '/portfolio',
		alias: '/',
		name: 'Portfolio',
		component: PortfolioView,
	},
	{
		path: '/tags',
		name: 'Tags',
		component: TagsView,
	},
	{
		path: '/dashboards',
		name: 'Dashboards',
		component: DashboardsView,
	},
	{
		path: '/custom',
		name: 'Custom',
		component: CustomView,
	},
	{
		path: '/custom/broker/:broker/account/:account/income',
		name: 'Add Income',
		component: AddIncomeView,
	},
	{
		path: '/custom/broker/:broker/account/:account/expense',
		name: 'Add Expense',
		component: AddExpenseView,
	},
	{
		path: '/custom/broker/:broker/asset/buy',
		name: 'Buy Asset',
		component: BuyAssetView,
	},
	{
		path: '/custom/broker/:broker/asset/sell',
		name: 'Sell Asset',
		component: SellAssetView,
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
