<template>
	<b>{{ broker.displayName }}</b>
	<p>
		Accounts:
		<accountList :broker-id="broker.id" :accounts="broker.accounts" />
	</p>
	<p>
		Assets:
		<assetList :assets="broker.inventory" :broker-id="broker.id" />
		<router-link :to="`/broker/${broker.id}/asset/buy`" class="btn btn-primary mb-2">Buy</router-link>
		<router-link :to="`/broker/${broker.id}/asset/sell`" class="btn btn-primary mb-2 ml-2">Sell</router-link>
	</p>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { BrokerDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import AccountList from '@/component/accountList.vue';
import AssetList from '@/component/assetList.vue';

@Options({
	name: 'Broker',
	components: {
		AccountList,
		AssetList,
	},
})
export default class Broker extends Vue {
	@Prop()
	broker!: BrokerDto;
}
</script>
