<template>
	<h2>Portfolio</h2>
	<h4 v-for="balance in virtualState.balances" :key="balance.currency">
		<money :value="balance.virtualPrice" :currency-id="balance.currency" />&#160;
		<money-diff :old="balance.realPrice" :new="balance.virtualPrice" :currency-id="balance.currency" />&#160;
	</h4>
	<virtual-asset-list :assets="inventory" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import VirtualAssetList from '@/component/virtualAssetList.vue';
import Money from '@/component/money.vue';
import MoneyDiff from '@/component/moneyDiff.vue';

@Options({
	name: 'VirtualStateView',
	components: {
		VirtualAssetList,
		Money,
		MoneyDiff,
	},
})
export default class VirtualStateView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	get inventory() {
		return this.virtualState.inventory;
	}
}
</script>
