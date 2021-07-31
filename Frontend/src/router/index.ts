import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import LoginView from '@/view/common/loginView.vue';
import RegisterView from '@/view/common/registerView.vue';
import Backend from '@/service/backend';
import AddBrokerView from '@/view/config/addBrokerView.vue';
import AddAccountView from '@/view/config/addAccountView.vue';
import ConfigView from '@/view/config/configView.vue';
import ImportView from '@/view/import/importView.vue';
import OperationsView from '@/view/operation/operationsView.vue';
import TagsView from '@/view/tag/tagsView.vue';
import PortfolioView from '@/view/portfolio/portfolioView.vue';
import DividendsView from '@/view/dividend/dividendsView.vue';
import DashboardsView from '@/view/dashboard/dashboardsView.vue';
import AddIncomeView from '@/view/custom/addIncomeView.vue';
import AddExpenseView from '@/view/custom/addExpenseView.vue';
import BuyAssetView from '@/view/custom/buyAssetView.vue';
import SellAssetView from '@/view/custom/sellAssetView.vue';
import CustomView from '@/view/custom/customView.vue';

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
		path: '/dividends',
		alias: '/',
		name: 'Dividends',
		component: DividendsView,
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
