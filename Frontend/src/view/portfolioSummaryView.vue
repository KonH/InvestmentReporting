<template>
	<portfolio-account-view />
	<portfolio-exchange-view />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioExchangeView from '@/view/portfolioExchangeView.vue';
import PortfolioAccountView from '@/view/portfolioAccountView.vue';

@Options({
	name: 'PortfolioSummaryView',
	components: {
		PortfolioAccountView,
		PortfolioExchangeView,
	},
})
export default class PortfolioSummaryView extends Vue {}
</script>
