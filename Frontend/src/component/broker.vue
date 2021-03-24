<template>
	<b>{{ broker.displayName }}</b>
	<accountList :broker-id="broker.id" :accounts="broker.accounts" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { BrokerDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import AccountList from '@/component/accountList.vue';

@Options({
	name: 'Broker',
	components: {
		AccountList,
	},
})
export default class Broker extends Vue {
	@Prop()
	broker!: BrokerDto;
}
</script>
